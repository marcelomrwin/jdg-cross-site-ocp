using System;
using EmployeeNetCoreApp.Model;

namespace EmployeeNetCoreApp.Services
{
	public interface IEmployeeService
	{
		public Task<IEnumerable<Employee>> GetEmployees();
		public Task<Employee?> GetEmployee(long id);
		public Task<Employee> SaveEmployee(Employee employee);
        public Task<Employee> UpdateEmployee(Employee employee,bool updateVersion = true);
		public Task DeleteEmployee(long id);		
		public Task<ISet<string>> GetAllEmployeesKeysInCache();

		public Task UpdateEntityFromCache(long employeeId);

		public Task<long> ImportEntityFromCache(string uuid);
		public Task<Employee> GetEmployeeByUUID(string uuid);
	}
}

