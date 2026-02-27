using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;

namespace StudentPayments_API.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> AuthenticateAsync(AuthRequestDto dto);
}