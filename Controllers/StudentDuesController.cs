using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.Models.Enums;
using StudentPayments_API.Services.Interfaces;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]

public class StudentDuesController : ControllerBase
{
    private readonly IStudentDuesService _studentDuesService;
    private readonly ILogger<StudentDuesController> _logger;
    public StudentDuesController(IStudentDuesService studentDuesService, ILogger<StudentDuesController> logger)
    {
        _studentDuesService = studentDuesService;
        _logger = logger;
    }
    [Authorize(Roles ="Admin")]
    [HttpPost]
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
    [Authorize]
    [HttpGet("balance")]
    public async Task<IActionResult> GetMyBalance()
    {
        var studentIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        if(studentIdClaim == null || !int.TryParse(studentIdClaim.Value, out int studentId))
        {
            return Unauthorized(new
            {
                message = "Invalid token: Student ID Claim is missing or invalid."
            });
        }
        var balance = await _studentDuesService.GetStudentBalanceAsync(new GetStudentBalanceRequestDto { StudentId = studentId });
        return Ok(new
        {
            balance
        });
    }
}