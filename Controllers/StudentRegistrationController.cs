using Microsoft.AspNetCore.Mvc;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Models;
using StudentPayments_API.Services.Interfaces;

namespace StudentPayments_API.Controllers;

//Controller to receive request, delegate logic to service, and return valid HTTP response.

[ApiController]
[Route("api/[controller]")]
public class StudentRegistrationController : ControllerBase
{
    private readonly IStudentRegistrationService _registrationService;
    
    //Constructor receives an implementation of IStudentRegistrationService via Dependency Injection
    public StudentRegistrationController(IStudentRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    //Expose POST endpoint: POST api/StudentRegistration/register
    [HttpPost("register")]
    public async Task<IActionResult> RegisterStudent([FromBody] StudentRegistrationDto dto)
    {
        //Call RegisterStudentAsync to attempt to register the student
        var (success, message, student) = await _registrationService.RegisterStudentAsync(dto);
        if (!success)
        {
            //If registration fails, return 400 bad request
            return BadRequest(new
            {
                message
            });
        }
        return Ok(new
        {
            //If registration succeeds, return 200 ok with student details
            message,
            student
        });
    }
}
