using Microsoft.AspNetCore.Mvc;
using Sms.Api.Data;

namespace Sms.Api.Controllers;

[ApiController]
[Route("employees")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeRepository _repository;

    public EmployeesController(EmployeeRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        return Ok(await _repository.ListAsync());
    }

    [HttpGet("{id:int}/phones")]
    public async Task<IActionResult> GetPhones(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Employee ID must be positive.");
        }

        return Ok(await _repository.GetPhonesAsync(id));
    }
}
