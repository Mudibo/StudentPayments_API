using Microsoft.AspNetCore.Mvc;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.Models;
using StudentPayments_API.Models.Enums;
using StudentPayments_API.Services.Interfaces;

namespace StudentPayments_API.Controllers;

//Controller to receive request, delegate logic to service, and return valid HTTP response.

[ApiController]
[Route("api/students")]
public class StudentRegistrationController : ControllerBase
{
    private readonly IStudentRegistrationService _registrationService;
    private readonly ILogger<StudentRegistrationController> _logger;
    
    //Constructor receives an implementation of IStudentRegistrationService via Dependency Injection
    public StudentRegistrationController(IStudentRegistrationService registrationService, ILogger<StudentRegistrationController> logger)
    {
        _registrationService = registrationService;
        _logger = logger;
    }

    //Expose POST endpoint: POST api/students/register
    [HttpPost("register")]
    public async Task<IActionResult> RegisterStudent([FromBody] StudentRegistrationDto dto)
    {
        if (!ModelState.IsValid)
        {
            //Return all validation errors
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new ApiErrorDto
            {
                error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                error_description = "Validation failed: " + string.Join("; ", errors)
            });
        }
        try {
            var response = await _registrationService.RegisterStudentAsync(dto);
            if(response.Success){
                return Ok(new {
                    success = response.Success,
                    message = response.Message,
                });
            }else{
                if(response.Error == OAuthErrorEnum.Conflict.ToOAuthErrorString()){
                    return StatusCode(409, new ApiErrorDto
                    {
                        error = OAuthErrorEnum.Conflict.ToOAuthErrorString(),
                        error_description = response.Message
                    });
                }else if(response.Error == OAuthErrorEnum.InvalidRequest.ToOAuthErrorString()){
                    return BadRequest(new ApiErrorDto
                    {
                        error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                        error_description = response.Message
                    });
                }else if(response.Error == OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString()){
                    return StatusCode(503, new ApiErrorDto
                    {
                        error = OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString(),
                        error_description = response.Message
                    });
                }else{
                    return StatusCode(500, new ApiErrorDto
                    {
                        error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                        error_description = response.Message
                    });
                }
            }
        }catch(Exception ex){
            _logger.LogError(ex, "An unexpected error occurred during student registration.");
            return StatusCode(500, new ApiErrorDto
            {
                error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                error_description = "An unexpected error occurred. Please try again later."
            });
        }
    }
}

