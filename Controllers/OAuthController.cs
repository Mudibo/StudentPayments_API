using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Writers;
using StudentPayments_API.DTOs.Requests;
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
            return BadRequest(new OAuthTokenResponseDto
            {
                error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                error_description = "Invalid grant_type."
            });
        }
        
        //Check if authorization header exists
        if(!Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            return Unauthorized(new OAuthTokenResponseDto
            {
                error = OAuthErrorEnum.InvalidRequest.ToOAuthErrorString(),
                error_description = "Authorization header is missing."
            });
        }
        
        //Ensure header starts with "Basic" 
        var header = authHeader.ToString();
        if (!header.StartsWith("Basic"))
        {
            return Unauthorized(new
            {
                error = "Invalid Authorization header"
            });
        }

        //Extract and decode client credentials from the header
        var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(header.Replace("Basic ", ""))).Split(':');
        if (credentials.Length != 2)
        {
            return Unauthorized(new
            {
                error = "Invalid Authorization header"
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
        var result = await _bankClientService.AuthenticateOAuthClientAsync(dto);
        if (!string.IsNullOrEmpty(result.error))
        {
            return result.error switch
            {
                "invalid_client" => Unauthorized(new OAuthTokenResponseDto 
                {
                    error = result.error,
                    error_description = "Invalid client credentials."
                }),
                "invalid_scope" => BadRequest(new OAuthTokenResponseDto
                {
                    error = result.error,
                    error_description = "Invalid scope"
                }),
                "temporarily_unavailable" => StatusCode(503, new OAuthTokenResponseDto
                {
                    error = result.error
                }),
                "server_error" => StatusCode(503, new OAuthTokenResponseDto
                {
                    error = result.error
                }),
                "unsupported_grant_type" => BadRequest(new OAuthTokenResponseDto
                {
                    error = result.error
                }),
                "invalid_request" => BadRequest(new OAuthTokenResponseDto
                {
                    error = result.error
                })
            };
        }
        return Ok(result);
    }
}