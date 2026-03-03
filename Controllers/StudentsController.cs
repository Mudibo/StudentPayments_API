using Microsoft.AspNetCore.Authorization;
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
public class StudentsController : ControllerBase
{
    private readonly IStudentRegistrationService _registrationService;
    private readonly IStudentDuesService _studentDuesService;
    private readonly IStudentValidationService _validationService;
    private readonly IStudentService _studentService;
    private readonly ILogger<StudentsController> _logger;
    
    //Constructor receives an implementation of IStudentRegistrationService via Dependency Injection
    public StudentsController(IStudentRegistrationService registrationService, IStudentDuesService studentDuesService, ILogger<StudentsController> logger, IStudentValidationService validationService, IStudentService studentService)
    {
        _registrationService = registrationService;
        _studentDuesService = studentDuesService;
        _validationService = validationService;
        _studentService = studentService;
        _logger = logger;
    }
    [HttpPost]
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
        try 
        {
            var response = await _registrationService.RegisterStudentAsync(dto);
            if(response.Success)
            {
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
    [Authorize(Roles = "Admin")]
    [HttpPost("dues")]
    public async Task<IActionResult>AddStudentDues([FromBody] AddStudentDuesDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiErrorDto
            {
                error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                error_description = "Invalid request data. Please ensure all required fields are provided and valid."
            });
        }
        try
        {
            var response = await _studentDuesService.AddDuesAsync(dto);
            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                if(response.Error == OAuthErrorEnum.NotFound.ToOAuthErrorString())
                {
                    return NotFound(new ApiErrorDto
                    {
                        error = OAuthErrorEnum.NotFound.ToOAuthErrorString(),
                        error_description = response.Message
                    });
                }else if(response.Error == OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString())
                {
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
        }catch(Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while processing the request.");
            return StatusCode(500, new ApiErrorDto
            {
                error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                error_description = $"An unexpected error occurred: {ex.Message}"
            });
        }
    }
    [Authorize(Policy ="StudentValidation")]
    [HttpPost("validate")]
    //async as the validation service performs asynchronous work (Database Access)
    public async Task<IActionResult> ValidateStudent([FromBody] StudentValidationRequestDto dto)
    {
        if(string.IsNullOrEmpty(dto.AdmissionNumber)){
            return BadRequest(new ApiErrorDto
            {
                error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                error_description = "Admission number is required."
            });
        }
        var response = await _validationService.ValidateStudentAsync(dto);
        switch(response.Status)
        {
            case Models.StudentValidationStatus.Valid:
                return Ok(response);
            case Models.StudentValidationStatus.NotFound:
                return NotFound(new ApiErrorDto {
                    error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                    error_description = response.Message
                });
            case Models.StudentValidationStatus.Inactive:
                return StatusCode(403, new ApiErrorDto {
                    error = OAuthErrorEnum.Inactive.ToOAuthErrorString(),
                    error_description = response.Message
                });
            case Models.StudentValidationStatus.TransientError:
                return StatusCode(503, new ApiErrorDto {
                    error = OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString(),
                    error_description = response.Message
                });
            case Models.StudentValidationStatus.Error:
                return BadRequest(new ApiErrorDto {
                    error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                    error_description = response.Message
                });
            default:
                return StatusCode(500, new ApiErrorDto
                {
                    error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                    error_description = "An unexpected error occurred during student validation."
                });
        }
    }
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllStudents([FromQuery] GetStudentsRequestDto dto)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(new ApiErrorDto
            {
                error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                error_description = "Invalid request parameters."
            });
        }
        try
        {
            if(dto.Page < 1) dto.Page = 1;
            if(dto.PageSize < 1 || dto.PageSize > 200) dto.PageSize = 50;
            
            var allStudents = await _studentService.GetStudentsAsync(dto);
            return Ok(allStudents);
        }catch(Exception ex)
        {
            _logger.LogError("An unexpected error occurred while retrieving students. ExceptionType:{ExceptionType}, ExceptionName: {ExceptionName}, StackTrace:{StackTrace}", ex.GetType().Name, ex.Message, ex.StackTrace);
            return StatusCode(500, new ApiErrorDto
            {
                error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                error_description = "An unexpected error occurred while retrieving students. Please try again."
            });
        }
    }
}

