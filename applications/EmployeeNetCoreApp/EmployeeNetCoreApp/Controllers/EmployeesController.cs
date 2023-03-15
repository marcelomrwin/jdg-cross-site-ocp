using EmployeeNetCoreApp.Data;
using EmployeeNetCoreApp.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeNetCoreApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly DataContext context;

    private readonly ILogger<EmployeesController> logger;

    public EmployeesController(DataContext pContext, ILogger<EmployeesController> pLogger)
    {
        context = pContext;
        logger = pLogger;
    }

    // GET: api/Employees
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
    {
        if (context.Employees == null)
        {
            return NotFound();
        }
        return await context.Employees.ToListAsync();
    }

    // GET: api/Employees/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> GetEmployee(int id)
    {
        if (context.Employees == null)
        {
            return NotFound();
        }
        var employee = await context.Employees.FindAsync(id);

        if (employee == null)
        {
            return NotFound();
        }

        return employee;
    }

    // PUT: api/Employees/1
    [HttpPut("{id}")]
    public async Task<IActionResult> PutEmployee(int id, Employee employee)
    {
        if (id != employee.EmployeeId)
        {
            return BadRequest();
        }

        context.Entry(employee).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EmployeeExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return Ok("Updated the Employee successfully !");
    }

    // POST: api/Employees
    [HttpPost]
    public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
    {
        if (context.Employees == null)
        {
            return Problem("Entity set 'DataContext.Employees'  is null.");
        }
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        return CreatedAtAction("GetEmployee", new { id = employee.EmployeeId }, employee);
    }

    // DELETE: api/Employees/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        if (context.Employees == null)
        {
            return NotFound();
        }
        var employee = await context.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        context.Employees.Remove(employee);
        await context.SaveChangesAsync();

        return Ok("Deleted the Employee successfully !");
    }

    private bool EmployeeExists(int id) 
    {
        return (context.Employees?.Any(e => e.EmployeeId == id)).GetValueOrDefault();
    }

}

