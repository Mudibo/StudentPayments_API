using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.Models;
using StudentPayments_API.Models.Enums;
using StudentPayments_API.Services.Interfaces;

namespace StudentPayments_API.Controllers;

///<summary>
/// Handles student registration, adding of student dues, student validation and retrieval
///</summary>
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
    /// <summary>
    /// Register a new student.
    /// </summary>
    /// <param name="dto">Student registration data</param>
    /// <response code="400">Bad Request</response>
    /// <response code="409">Student already exists.</response>
    /// <response code="500">Server error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status500InternalServerError)]
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
            if (response.Success)
            {
                return Ok(new
                {
                    success = response.Success,
                    message = response.Message,
                });
            }
            else
            {
                if (response.Error == OAuthErrorEnum.Conflict.ToOAuthErrorString())
                {
                    return StatusCode(409, new ApiErrorDto
                    {
                        error = OAuthErrorEnum.Conflict.ToOAuthErrorString(),
                        error_description = response.Message
                    });
                }
                else if (response.Error == OAuthErrorEnum.InvalidRequest.ToOAuthErrorString())
                {
                    return BadRequest(new ApiErrorDto
                    {
                        error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                        error_description = response.Message
                    });
                }
                else if (response.Error == OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString())
                {
                    return StatusCode(503, new ApiErrorDto
                    {
                        error = OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString(),
                        error_description = response.Message
                    });
                }
                else
                {
                    return StatusCode(500, new ApiErrorDto
                    {
                        error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                        error_description = response.Message
                    });
                }
            }
        }
        catch (Exception ex)
        {
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
    public async Task<IActionResult> AddStudentDues([FromBody] AddStudentDuesDto dto)
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
                if (response.Error == OAuthErrorEnum.NotFound.ToOAuthErrorString())
                {
                    return NotFound(new ApiErrorDto
                    {
                        error = OAuthErrorEnum.NotFound.ToOAuthErrorString(),
                        error_description = response.Message
                    });
                }
                else if (response.Error == OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString())
                {
                    return StatusCode(503, new ApiErrorDto
                    {
                        error = OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString(),
                        error_description = response.Message
                    });
                }
                else
                {
                    return StatusCode(500, new ApiErrorDto
                    {
                        error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                        error_description = response.Message
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while processing the request.");
            return StatusCode(500, new ApiErrorDto
            {
                error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                error_description = $"An unexpected error occurred: {ex.Message}"
            });
        }
    }
    [Authorize(Policy = "StudentValidation")]
    [HttpGet("{admissionNumber}/balance")]
    public async Task<IActionResult> GetStudentBalance([FromRoute] string admissionNumber)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new ApiErrorDto
            {
                error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                error_description = string.Join("; ", errors)
            });
        }
        try
        {
            var dto = new GetStudentBalanceRequestDto
            {
                AdmissionNumber = admissionNumber
            };
            var result = await _studentDuesService.GetStudentBalanceAsync(dto);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                if (result.Error == OAuthErrorEnum.NotFound.ToOAuthErrorString())
                {
                    return NotFound(new ApiErrorDto
                    {
                        error = OAuthErrorEnum.NotFound.ToOAuthErrorString(),
                        error_description = result.Message
                    });
                }
                else if (result.Error == OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString())
                {
                    return StatusCode(503, new ApiErrorDto
                    {
                        error = OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString(),
                        error_description = result.Message
                    });
                }
                else
                {
                    return StatusCode(500, new ApiErrorDto
                    {
                        error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                        error_description = result.Message
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving student balance.");
            return StatusCode(500, new ApiErrorDto
            {
                error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                error_description = "An unexpected error occurred while retrieving student balance. Please try again later."
            });
        }
    }
    [Authorize(Roles = "Admin")]
    [HttpGet("dues")]
    public async Task<IActionResult> GetAllStudentDues([FromQuery] GetStudentsDuesRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new ApiErrorDto
            {
                error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                error_description = string.Join("; ", errors)
            });
        }
        if (dto.PageSize > 50) dto.PageSize = 50;
        if (dto.Page < 1) dto.Page = 1;

        try
        {
            var result = await _studentDuesService.GetAllStudentDuesAsync(dto);
            if (result.TotalCount == 0)
            {
                return NotFound(new ApiErrorDto
                {
                    error = OAuthErrorEnum.NotFound.ToOAuthErrorString(),
                    error_description = "No student dues found."
                });
            }
            if (result.Error == null)
            {
                return Ok(result);
            }
            else
            {
                if (result.Error == OAuthErrorEnum.TemporarilyUnavailable)
                {
                    return StatusCode(503, new ApiErrorDto
                    {
                        error = OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString(),
                        error_description = result.Message
                    });
                }
                else if (result.Error == OAuthErrorEnum.ServerError)
                {
                    return StatusCode(500, new ApiErrorDto
                    {
                        error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                        error_description = result.Message
                    });
                }
                else
                {
                    return StatusCode(500, new ApiErrorDto
                    {
                        error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                        error_description = "An unexpected error occurred while retrieving student dues. Please try again."
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving student dues.");
            return StatusCode(500, new ApiErrorDto
            {
                error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                error_description = "An unexpected error occurred while retrieving student dues. Please try again later."
            });
        }
    }
    [Authorize(Policy = "StudentValidation")]
    [HttpPost("validate")]
    //async as the validation service performs asynchronous work (Database Access)
    public async Task<IActionResult> ValidateStudent([FromBody] StudentValidationRequestDto dto)
    {
        if (string.IsNullOrEmpty(dto.AdmissionNumber))
        {
            return BadRequest(new ApiErrorDto
            {
                error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                error_description = "Admission number is required."
            });
        }
        var response = await _validationService.ValidateStudentAsync(dto);
        switch (response.Status)
        {
            case Models.StudentValidationStatus.Valid:
                return Ok(response);
            case Models.StudentValidationStatus.NotFound:
                return NotFound(new ApiErrorDto
                {
                    error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                    error_description = response.Message
                });
            case Models.StudentValidationStatus.Inactive:
                return StatusCode(403, new ApiErrorDto
                {
                    error = OAuthErrorEnum.Inactive.ToOAuthErrorString(),
                    error_description = response.Message
                });
            case Models.StudentValidationStatus.TransientError:
                return StatusCode(503, new ApiErrorDto
                {
                    error = OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString(),
                    error_description = response.Message
                });
            case Models.StudentValidationStatus.Error:
                return BadRequest(new ApiErrorDto
                {
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
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiErrorDto
            {
                error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                error_description = "Invalid request parameters."
            });
        }
        try
        {
            if (dto.Page < 1) dto.Page = 1;
            if (dto.PageSize < 1 || dto.PageSize > 200) dto.PageSize = 50;

            var allStudents = await _studentService.GetStudentsAsync(dto);
            return Ok(allStudents);
        }
        catch (Exception ex)
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

