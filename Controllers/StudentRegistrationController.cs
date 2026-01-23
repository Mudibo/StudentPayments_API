using Microsoft.AspNetCore.Mvc;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Models;
using StudentPayments_API.Services.Interfaces;

namespace StudentPayments_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentRegistrationController : ControllerBase
{
    private readonly IStudentRegistrationService _registrationService;
    public StudentRegistrationController(IStudentRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }
    [HttpPost("register")]
    public async Task<IActionResult> RegisterStudent([FromBody] StudentRegistrationDto dto)
    {
        var (success, message, student) = await _registrationService.RegisterStudentAsync(dto);
        if (!success)
        {
            return BadRequest(new
            {
                message
            });
        }
        return Ok(new
        {
            message,
            student
        });
    }
}
