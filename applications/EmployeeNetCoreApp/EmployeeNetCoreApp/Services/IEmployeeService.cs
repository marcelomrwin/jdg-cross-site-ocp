using System;
using EmployeeNetCoreApp.Model;

namespace EmployeeNetCoreApp.Services
{
	public interface IEmployeeService
	{
		public Task<IEnumerable<Employee>> GetEmployees();
		public Task<Employee?> GetEmployee(int id);
		public Task<Employee> SaveEmployee(Employee employee);
        public Task<Employee> UpdateEmployee(Employee employee,bool updateVersion = true);
		public Task DeleteEmployee(int id);

		public Task SyncCacheWithDatabase(CancellationToken cancellationToken);
	}
}

