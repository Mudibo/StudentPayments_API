using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Writers;
using StudentPayments_API.DTOs.Requests;
using StudentPayments_API.DTOs.Responses;
using StudentPayments_API.Models.Enums;
using StudentPayments_API.Services.Interfaces;
using System.Text;

[ApiController]
[Route("api/oauth")]

public class OAuthController : ControllerBase
{
    private readonly IBankClientService _bankClientService;

    public OAuthController(IBankClientService bankClientService)
    {
        _bankClientService = bankClientService;
    }

    [HttpPost("token")]
    public async Task<IActionResult> Token([FromBody] OAuthClientAuthRequestDto dto)
    {
        if (dto == null || string.IsNullOrEmpty(dto.GrantType) || dto.GrantType != "client_credentials")
        {
            return BadRequest(new OAuthErrorResponseDto
            {
                error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                error_description = "The grant_type provided is invalid."
            });
        }

        // Authenticate the client using the bank client service
        try
        {
            var result = await _bankClientService.AuthenticateOAuthClientAsync(dto);
            return Ok(result);
        }
        catch (OAuthException ex)
        {
            return ex.Error switch
            {
                "invalid_client" => Unauthorized(new OAuthErrorResponseDto
                {
                    error = ex.Error,
                    error_description = ex.ErrorDescription
                }),
                "invalid_scope" => BadRequest(new OAuthErrorResponseDto
                {
                    error = ex.Error,
                    error_description = ex.ErrorDescription
                }),
                "temporarily_unavailable" => StatusCode(503, new OAuthErrorResponseDto
                {
                    error = ex.Error,
                    error_description = ex.ErrorDescription
                }),
                "server_error" => StatusCode(500, new OAuthErrorResponseDto
                {
                    error = ex.Error,
                    error_description = ex.ErrorDescription
                }),
                "invalid_request" => BadRequest(new OAuthErrorResponseDto
                {
                    error = ex.Error,
                    error_description = ex.ErrorDescription
                }),
                "unsupported_grant_type" => BadRequest(new OAuthErrorResponseDto
                {
                    error = ex.Error,
                    error_description = ex.ErrorDescription
                }),
                _ => StatusCode(500, new OAuthErrorResponseDto
                {
                    error = OAuthErrorEnum.ServerError.ToOAuthErrorString(),
                    error_description = "An unexpected error occurred during authentication."
                })
            };
        }
    }
}