using Microsoft.AspNetCore.Mvc;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Data;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using StudentPayments_API.Models;
using StudentPayments_API.Security.Interfaces;
using StudentPayments_API.Models.Enums;
using StudentPayments_API.Services.Interfaces;
using StudentPayments_API.DTOs.Responses;
namespace StudentPayments_API.Controllers;

[ApiController] 
[Route("api/auth")] 
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequestDto dto)
    {
        if(!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new ApiErrorDto
            {
                error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                error_description = "Validation failed: " + string.Join("; ", errors)
            });
        }
        try
        {
            var response = await _authService.AuthenticateAsync(dto);
            if (response.success)
            {
                return Ok(response);
            }
            else
            {
                if(response.error == OAuthErrorEnum.InvalidClient.ToOAuthErrorString())
                {
                    return Unauthorized(new ApiErrorDto
                    {
                        error = OAuthErrorEnum.InvalidClient.ToOAuthErrorString(),
                        error_description = "Invalid credentials"
                    });
                }
                else if(response.error == OAuthErrorEnum.Inactive.ToOAuthErrorString())
                {
                    return Unauthorized(new ApiErrorDto
                    {
                        error = OAuthErrorEnum.Inactive.ToOAuthErrorString(),
                        error_description = "Student account is inactive."
                    });
                }
                else if(response.error == OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString())
                {
                    return StatusCode(503, new ApiErrorDto
                    {
                        error = OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString(),
                        error_description = "A database error occurred during authentication. Please try again."
                    });
                }
                else
                {
                    return StatusCode(500, new ApiErrorDto
                    {
                        error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                        error_description = "An unexpected error occurred during authentication. Please try again later."
                    });
                }
            }
        }catch(Exception ex)
        {
            _logger.LogError("An unexpected error occurred during authentication. ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", ex.GetType().Name, ex.StackTrace);
            return StatusCode(500, new ApiErrorDto
            {
                error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                error_description = "An unexpected error occurred during authentication. Please try again later."
            });
        }
    }
    
        
}