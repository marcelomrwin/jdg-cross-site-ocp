using System.Text.Json;
using EmployeeNetCoreApp.Data;
using EmployeeNetCoreApp.Exceptions;
using EmployeeNetCoreApp.Model;
using EmployeeNetCoreApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeNetCoreApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService employeeService;
    private readonly ILogger<EmployeesController> logger;

    public EmployeesController(IEmployeeService pEmployeeService, ILogger<EmployeesController> pLogger)
    {
        logger = pLogger;
        employeeService = pEmployeeService;
        logger.LogWarning(this + " created and configured");
    }

    // GET: api/Employees
    [HttpGet]
    public async Task<IEnumerable<Employee>> GetEmployees()
    {
        return await employeeService.GetEmployees();
    }

    // GET: api/Employees/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> GetEmployee(int id)
    {
        var employee = await employeeService.GetEmployee(id);

        if (employee == null)
        {
            return NotFound();
        }

        return employee;
    }

    // PUT: api/Employees/1
    [HttpPut("{id}")]
    public async Task<IActionResult> PutEmployee(long id, Employee employee)
    {
        if (id != employee.EmployeeId)
        {
            logger.LogWarning("EmployeeId must not be null or different of {id}", id);
            return BadRequest();
        }

        try
        {
            await employeeService.UpdateEmployee(employee);
        }
        catch (EntityOutdatedException eoe)
        {
            return BadRequest(eoe.Message());
        }
        catch (DbUpdateException dbue)
        {
            return NotFound(dbue.Message);
        }

        return Ok(employee);
    }

    // POST: api/Employees
    [HttpPost]
    public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
    {
        try
        {
            await employeeService.SaveEmployee(employee);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.StackTrace);
            string message = string.Format("Fail during saving entity {0} with exception {1}", JsonSerializer.Serialize(employee), ex.ToString());
            return Problem(message);
        }

        return CreatedAtAction("GetEmployee", new { id = employee.EmployeeId }, employee);
    }

    // DELETE: api/Employees/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(long id)
    {
        try
        {
            await employeeService.DeleteEmployee(id);
        }
        catch (DbUpdateException dbue)
        {
            return NotFound(dbue.Message);
        }

        return Ok("Deleted the Employee successfully !");
    }

    [HttpOptions]
    public async Task<ISet<string>> GetAllEmployeesKeysInCache()
    {
        return await employeeService.GetAllEmployeesKeysInCache();
    }

    // PUT: api/employees/fromcache/1
    [HttpPut("fromcache/{employeeId}")]
    public async Task<IActionResult> UpdateEmployeeFromCache(long employeeId)
    {        
        try
        {
            await employeeService.UpdateEntityFromCache(employeeId);
        }
        catch (Exception e)
        {
            return NotFound(e.Message);
        }

        return Ok("Employee Updated successfully!");

    }

    // POST: api/employees/fromcache/abc-123
    [HttpPost("fromcache/{employeeId}")]
    public async Task<IActionResult> ImportEmployeeFromCache(string uuid)
    {
        try
        {
            await employeeService.ImportEntityFromCache(uuid);
        }
        catch (Exception e)
        {
            return NotFound(e.Message);
        }

        return StatusCode(201, "Employee Updated successfully!");
    }

}

