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
    public async Task<IActionResult> Token([FromForm] string scope, [FromForm] string grant_type)
    {
        if(string.IsNullOrEmpty(grant_type) || grant_type!= "client_credentials")
        {
            return BadRequest(new OAuthErrorResponseDto
            {
                error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                error_description = "The grant_type provided is invalid."
            });
        }
        
        //Check if authorization header exists
        if(!Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            return Unauthorized(new OAuthErrorResponseDto
            {
                error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                error_description = "Authorization header is missing."
            });
        }
        
        //Ensure header starts with "Basic" 
        var header = authHeader.ToString();
        if (!header.StartsWith("Basic"))
        {
            return Unauthorized(new OAuthErrorResponseDto
            {
                error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                error_description = "Authorization header provided is invalid."
            });
        }

        //Extract and decode client credentials from the header
        var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(header.Replace("Basic ", ""))).Split(':');
        if (credentials.Length != 2)
        {
            return Unauthorized(new OAuthErrorResponseDto
            {
                error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                error_description = "Authorization header format is invalid."
            });
        }
        var clientId = credentials[0];
        var clientSecret = credentials[1];
        var dto = new OAuthClientAuthRequestDto
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            Scope = scope,
            GrantType = grant_type
        };

        //Authenticate the client using the bank client service
        try
        {
            var result = await _bankClientService.AuthenticateOAuthClientAsync(dto);
            return Ok(result);
        }catch(OAuthException ex)
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
            };
        }
    }
}