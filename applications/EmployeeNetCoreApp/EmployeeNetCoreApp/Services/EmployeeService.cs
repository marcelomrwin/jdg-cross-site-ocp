using EmployeeNetCoreApp.Data;
using EmployeeNetCoreApp.Model;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using EmployeeNetCoreApp.Cache;
using System.Text.Json;
using EmployeeNetCoreApp.Exceptions;

namespace EmployeeNetCoreApp.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly DataContext context;

        private readonly ILogger<EmployeeService> logger;
        private readonly DataGridRestClient dataGridRestClient;


        public EmployeeService(DataContext pContext, DataGridRestClient gridRestClient, ILogger<EmployeeService> pLogger)
        {
            context = pContext;
            this.dataGridRestClient = gridRestClient;
            logger = pLogger;
        }

        public async Task<Employee?> GetEmployee(long id)
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
            employee.UpdatedBy = "DotNetUser";
            employee.UpdatedDate = DateTime.UtcNow;
            employee.Version = 1;

            if (!IsNotNull(employee.UUID))
                employee.UUID = Guid.NewGuid().ToString();

            context.Employees.Add(employee);
            await context.SaveChangesAsync();

            EmployeeDTO employeeDTO = EmployeeDTO.FromEntity(employee);
            employeeDTO.State = EmployeeDTO.CREATED;
            dataGridRestClient.AddtoCache(employee.UUID, employeeDTO.ToJson());

            return employee;

        }

        public async Task<Employee> UpdateEmployee(Employee employee, bool updateVersion = true)
        {
            if (!EmployeeExists(employee.EmployeeId))
                throw new DbUpdateException("Entity " + employee.EmployeeId + " not found in the local database");

            var dbEmployee = context.Employees.AsNoTracking().Where(e => e.EmployeeId == employee.EmployeeId).Select(e => e).Single();

            if (IsNotNull(dbEmployee))
            {
                EmployeeDTO? cacheEmployee = await dataGridRestClient.GetEmployeeFromCache(dbEmployee.UUID);
                if (IsNotNull(cacheEmployee))
                {
                    if (dbEmployee.Version < cacheEmployee.Version)
                    {
                        throw new EntityOutdatedException(dbEmployee.UUID, cacheEmployee.UpdatedBy, cacheEmployee.UpdatedDate, dbEmployee.Version, cacheEmployee.Version);
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

                EmployeeDTO employeeDTO = EmployeeDTO.FromEntity(employee);
                employeeDTO.State = EmployeeDTO.UPDATED;
                dataGridRestClient.AddtoCache(employee.UUID, employeeDTO.ToJson());
            }

            return employee;
        }

        public async Task DeleteEmployee(long id)
        {
            if (!EmployeeExists(id))
                throw new DbUpdateException("Entity " + id + " not found!");

            var employee = await context.Employees.FindAsync(id);
            if (IsNotNull(employee))
                context.Employees.Remove(employee);

            await context.SaveChangesAsync();

            dataGridRestClient.DeleteEmployeeFromCache(employee.UUID);
        }

        private bool EmployeeExists(long id)
        {
            return (context.Employees?.Any(e => e.EmployeeId == id)).GetValueOrDefault();
        }

        private bool EmployeeExists(string uuid)
        {
            return (context.Employees?.Any(e => e.UUID == uuid)).GetValueOrDefault();
        }

        private int GetEmployeeCurrentVersion(long id)
        {
            return context.Employees.Where(e => e.EmployeeId == id).Select(e => e.Version).SingleOrDefault();
        }

        private static bool IsNotNull([NotNullWhen(true)] object? obj) => obj != null;

        private Employee GetEmployeeByUUID(string uuid)
        {
            var dbEmployee = context.Employees.AsNoTracking().Where(e => e.UUID == uuid).Select(e => e).Single();
            return dbEmployee;
        }

        public Task<ISet<string>> GetAllEmployeesKeysInCache()
        {
            return dataGridRestClient.GetAllKeysFromCache();
        }

        public async Task UpdateEntityFromCache(long employeeId)
        {
            if (!EmployeeExists(employeeId))
                throw new Exception("Employee " + employeeId + " not found in the database!");

            var dbEmployee = context.Employees.AsNoTracking().Where(e => e.EmployeeId == employeeId).Select(e => e).Single();
            EmployeeDTO? cacheEmployee = await dataGridRestClient.GetEmployeeFromCache(dbEmployee.UUID);
            if (IsNotNull(cacheEmployee))
            {
                Employee employee = cacheEmployee.ToEntity();
                employee.EmployeeId = dbEmployee.EmployeeId;
                context.Entry(employee).State = EntityState.Modified;

                await context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Employee " + employeeId + " not found in the cache!");
            }

        }

        public async Task<long> ImportEntityFromCache(string uuid)
        {
            EmployeeDTO? cacheEmployee = await dataGridRestClient.GetEmployeeFromCache(uuid);
            if (IsNotNull(cacheEmployee))
            {
                Employee employee = cacheEmployee.ToEntity();
                if (EmployeeExists(uuid))
                {
                    //update
                    var dbEmployee = context.Employees.AsNoTracking().Where(e => e.UUID == uuid).Select(e => e).Single();
                    employee.EmployeeId = dbEmployee.EmployeeId;
                    context.Entry(employee).State = EntityState.Modified;

                    await context.SaveChangesAsync();
                    return dbEmployee.EmployeeId;
                }
                else
                {
                    //insert
                    context.Employees.Add(employee);
                    await context.SaveChangesAsync();
                    return employee.EmployeeId;
                }
            }
            return 0;
        }

    }
}

