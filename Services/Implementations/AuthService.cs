using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.Data;
using StudentPayments_API.Models;
using StudentPayments_API.Security.Interfaces;
using StudentPayments_API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using StudentPayments_API.Services.Interfaces;

namespace StudentPayments_API.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly StudentPaymentsDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(StudentPaymentsDbContext context, ITokenService tokenService, ILogger<AuthService> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }
    public async Task<AuthResponseDto> AuthenticateAsync(AuthRequestDto dto)
    {
        var trimmedAdmissionNumber = dto.AdmissionNumber?.Trim();
        var trimmedPassword = dto.Password?.Trim();
        try
        {
            var studentData = await _context.Students
                .Where(s => s.AdmissionNumber == trimmedAdmissionNumber)
                .Select(s => new { s.AdmissionNumber, s.PasswordHash, s.Role, s.EnrollmentStatus })
                .FirstOrDefaultAsync();
            if(studentData == null)
            {
                _logger.LogWarning("Authentication failed for admission number: {AdmissionNumber}", trimmedAdmissionNumber);
                return new AuthResponseDto
                {
                    success = false,
                    access_token = null,
                    token_type = null,
                    error = OAuthErrorEnum.InvalidClient.ToOAuthErrorString(),
                    expires_in = null       
                };
            }
            if(studentData.EnrollmentStatus == EnrollmentStatusEnum.Inactive)
            {
                _logger.LogWarning("Login Failed - Inactive Student: {AdmissionNumber}", trimmedAdmissionNumber);
                return new AuthResponseDto
                {
                    success = false,
                    access_token = null,
                    token_type = null,
                    error = OAuthErrorEnum.Inactive.ToOAuthErrorString(),
                    expires_in = null       
                };
            }
            if(!BCrypt.Net.BCrypt.Verify(trimmedPassword, studentData.PasswordHash))
            {
                _logger.LogWarning("Authentication failed for admission number: {AdmissionNumber} - Incorrect password", trimmedAdmissionNumber);
                return new AuthResponseDto
                {
                    success = false,
                    access_token = null,
                    token_type = null,
                    error = OAuthErrorEnum.InvalidClient.ToOAuthErrorString(),
                    expires_in = null       
                };
            }
            var tokenResponse = _tokenService.GenerateToken(new Student
            {
                AdmissionNumber = studentData.AdmissionNumber,
                Role = studentData.Role
            });
            return new AuthResponseDto
            {
                success = true,
                access_token = tokenResponse.Token,
                token_type = "Bearer",
                expires_in = (int)(tokenResponse.Expiration - DateTime.UtcNow).TotalSeconds,
                role = tokenResponse.Role
            };
        }catch(DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error occurred during authentication for admission number: {AdmissionNumber}", trimmedAdmissionNumber);
            return new AuthResponseDto
            {
                success = false,
                access_token = null,
                token_type = null,
                error = OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString(),
                expires_in = null       
            };
        }catch(InvalidOperationException inOps)
        {
            _logger.LogError("Invalid operation during authentication for admission number: {AdmissionNumber} ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", trimmedAdmissionNumber, inOps.GetType().Name, inOps.StackTrace);
            return new AuthResponseDto
            {
                success = false,
                access_token = null,
                token_type = null,
                error = OAuthErrorEnum.TemporarilyUnavailable.ToOAuthErrorString(),
                expires_in = null       
            };
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during authentication for admission number: {AdmissionNumber} ExceptionType: {ExceptionType}, StackTrace: {StackTrace}", trimmedAdmissionNumber, ex.GetType().Name, ex.StackTrace);
            return new AuthResponseDto
            {
                success = false,
                access_token = null,
                token_type = null,
                error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                expires_in = null       
            };
        }
    }
}