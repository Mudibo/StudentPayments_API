using System.Threading.Tasks;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.Models;
using StudentPayments_API.DTOs.Responses;

namespace StudentPayments_API.Services.Interfaces;
public interface IBankClientService
{
    Task<AddBankClientResponseDto> CreateBankClientAsync(CreateBankClientDto dto);
    Task<OAuthTokenResponseDto> AuthenticateOAuthClientAsync(OAuthClientAuthRequestDto dto);
}