using EmployeeNetCoreApp.Data;
using EmployeeNetCoreApp.Model;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using EmployeeNetCoreApp.Cache;
using Infinispan.Hotrod.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Microsoft.Extensions.Caching.Distributed;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using EmployeeNetCoreApp.Exceptions;

namespace EmployeeNetCoreApp.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly DataContext context;
        private readonly CacheConfiguration cacheConfig;
        private readonly ILogger<EmployeeService> logger;
        private readonly IDistributedCache distributedCache;
        private readonly InfinispanDG infinispanDG;

        private Cache<string, string> cache { get; }

        public EmployeeService(DataContext pContext, IDistributedCache distCache, CacheConfiguration pCacheConfig, InfinispanDG dG, ILogger<EmployeeService> pLogger)
        {
            context = pContext;
            cacheConfig = pCacheConfig;
            logger = pLogger;
            distributedCache = distCache;
            infinispanDG = dG;
            cache = infinispanDG.NewCache(new StringMarshaller(), new StringMarshaller(), cacheConfig.Cache);
        }

        public async Task<Employee?> GetEmployee(int id)
        {

            if (context.Employees == null)
            {
                return null;
            }

            var employee = await context.Employees.FindAsync(id);

            if (employee == null)
            {
                return null;
            }

            return employee;

        }

        public async Task<IEnumerable<Employee>> GetEmployees()
        {
            if (context.Employees == null)
            {
                return Enumerable.Empty<Employee>().ToList();
            }
            return await context.Employees.ToListAsync();
        }

        public async Task<Employee> SaveEmployee(Employee employee)
        {

            if (context.Employees == null)
            {
                throw new Exception();
            }

            employee.CreatedBy = "DotNetUser";
            employee.CreateDate = DateTime.UtcNow;
            employee.Version = 1;

            if (!IsNotNull(employee.UUID))
                employee.UUID = Guid.NewGuid().ToString();

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            logger.LogInformation("Entity Saved! Updating Cache");

            string json = JsonSerializer.Serialize(employee);
            distributedCache.SetString(employee.UUID, json);

            logger.LogInformation("Cache Updated!");

            return employee;

        }

        public async Task<Employee> UpdateEmployee(Employee employee, bool updateVersion = true)
        {
            if (!EmployeeExists(employee.EmployeeId))
                throw new DbUpdateException("Entity " + employee.EmployeeId + " not found!");

            //implements the logic to check the version before update (compare version number)

            var dbEmployee = context.Employees.AsNoTracking().Where(e => e.EmployeeId == employee.EmployeeId).Select(e => e).Single();

            if (IsNotNull(dbEmployee))
            {

                if (await UserExistsInCacheAsync(dbEmployee.UUID))
                {
                    Employee cacheEmployee = GetEmployeeFromCache(dbEmployee.UUID);
                    if (dbEmployee.Version < cacheEmployee.Version)
                    {
                        throw new EntityOutdatedException(dbEmployee.UUID,cacheEmployee.UpdatedBy,cacheEmployee.UpdatedDate,dbEmployee.Version,cacheEmployee.Version);
                    }
                }

                context.Entry(employee).State = EntityState.Modified;

                employee.UpdatedBy = "DotNetUser";
                employee.UpdatedDate = DateTime.UtcNow;
                employee.UUID = dbEmployee.UUID;
                employee.CreateDate = dbEmployee.CreateDate;
                employee.CreatedBy = dbEmployee.CreatedBy;
                if (updateVersion)
                    employee.Version = dbEmployee.Version + 1;

                await context.SaveChangesAsync();

                string json = JsonSerializer.Serialize(employee);
                distributedCache.SetString(employee.UUID, json);
                distributedCache.Refresh(employee.UUID);
            }

            return employee;
        }

        private async Task<Employee> UpdateEmployeeFromSchedule(Employee employee)
        {
            if (!EmployeeExists(employee.EmployeeId))
                throw new DbUpdateException("Entity " + employee.EmployeeId + " not found!");

            var dbEmployee = context.Employees.AsNoTracking().Where(e => e.EmployeeId == employee.EmployeeId).Select(e => e).Single();

            if (IsNotNull(dbEmployee))
            {
                
                context.Entry(employee).State = EntityState.Modified;

                employee.UpdatedBy = "DotNetUser";
                employee.UpdatedDate = DateTime.UtcNow;
                employee.UUID = dbEmployee.UUID;
                employee.CreateDate = dbEmployee.CreateDate;
                employee.CreatedBy = dbEmployee.CreatedBy;                
                await context.SaveChangesAsync();
                
            }

            return employee;
        }

        public async Task DeleteEmployee(int id)
        {
            if (!EmployeeExists(id))
                throw new DbUpdateException("Entity " + id + " not found!");

            var employee = await context.Employees.FindAsync(id);
            if (IsNotNull(employee))
                context.Employees.Remove(employee);

            await context.SaveChangesAsync();
        }

        private bool EmployeeExists(int id)
        {
            return (context.Employees?.Any(e => e.EmployeeId == id)).GetValueOrDefault();
        }

        private bool EmployeeExists(string uuid)
        {
            return (context.Employees?.Any(e => e.UUID == uuid)).GetValueOrDefault();
        }

        private int GetEmployeeCurrentVersion(int id)
        {
            return context.Employees.Where(e => e.EmployeeId == id).Select(e => e.Version).SingleOrDefault();
        }

        private static bool IsNotNull([NotNullWhen(true)] object? obj) => obj != null;

        private Employee GetEmployeeByUUID(string uuid)
        {
            var dbEmployee = context.Employees.AsNoTracking().Where(e => e.UUID == uuid).Select(e => e).Single();
            return dbEmployee;
        }

        private async Task<bool> UserExistsInCacheAsync(string uuid)
        {
            return await cache.ContainsKey(uuid);
        }

        public async Task SyncCacheWithDatabase(CancellationToken cancellationToken)
        {

            //Cache<string, string> cache = infinispanDG.NewCache(new StringMarshaller(), new StringMarshaller(), cacheConfig.Cache);
            ISet<string> keys = await cache.KeySet();
            foreach (string key in keys)
            {

                Employee employee = GetEmployeeFromCache(key);

                if (EmployeeExists(key))
                {
                    Employee localEmployee = GetEmployeeByUUID(key);
                    if (localEmployee.Version < employee.Version)
                    {
                        logger.LogInformation("Update employee {}", key);
                        employee.EmployeeId = localEmployee.EmployeeId;
                        await UpdateEmployeeFromSchedule(employee);
                    }
                }
                else
                {
                    logger.LogInformation("Saving employee {}", key);
                    employee.EmployeeId = 0;
                    await SaveEmployee(employee);
                }
            }

        }

        private Employee GetEmployeeFromCache(string key)
        {
            return JsonSerializer.Deserialize<Employee>(distributedCache.GetString(key));            
        }
    }
}

