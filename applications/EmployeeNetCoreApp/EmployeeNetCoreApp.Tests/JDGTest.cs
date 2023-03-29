using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using EmployeeNetCoreApp.Cache;
using EmployeeNetCoreApp.Model;
using Xunit.Abstractions;

namespace EmployeeNetCoreApp.Tests;

public class JDGTest
{
    private static readonly string UUID = "bdf5b0b6-a1ed-49cc-9181-30cba64bd5e6";
    private readonly ITestOutputHelper output;
    private readonly DataGridRestClient dataGridRestClient;

    public JDGTest(ITestOutputHelper output)
    {
        this.output = output;
        CacheConfiguration configuration = new CacheConfiguration();
        configuration.Cache = "employees";
        configuration.Host = "<DATAGRID-ROUTE>-<SITE1-PROJECT>.<SITE1-DOMAIN>";
        configuration.Protocol = "https";
        configuration.Port = 443;
        configuration.User = "admin";
        configuration.Password = "password";
        this.dataGridRestClient = new DataGridRestClient(configuration);
    }

    [Fact]
    public void ConnectToJDG()
    {
        try
        {
            Employee employee = new Employee();
            employee.FullName = "Employee Test";
            employee.Department = "IT";
            employee.Designation = "Designation Test";
            employee.CreatedBy = "Xunit Test";
            employee.CreateDate = DateTime.UtcNow;
            employee.UUID = UUID;

            EmployeeDTO employeeDTO = EmployeeDTO.FromEntity(employee);
            employeeDTO.State = EmployeeDTO.CREATED;
            string employeeJson = employeeDTO.ToJson();

            output.WriteLine(employeeJson);

            dataGridRestClient.AddtoCache(UUID, employeeJson);

            var cacheEmployee = dataGridRestClient.GetEmployeeFromCache(UUID);

            output.WriteLine(cacheEmployee.ToString());
            Assert.NotNull(cacheEmployee);

        }
        catch (Exception e)
        {
            output.WriteLine(e.ToString());
            Xunit.Assert.Fail(e.Message);
        }
        System.Console.Write("Test End");
    }
    [Fact]
    public async void testSync()
    {
        ISet<string> keys = await dataGridRestClient.GetAllKeysFromCache();
        foreach (string key in keys)
        {
            output.WriteLine(key);
        }
    }
}