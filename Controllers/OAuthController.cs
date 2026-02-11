using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> Token(
        [FromForm] string grant_type, 
        [FromForm] string scope
    )
    {
        //Only accepts client_credentials grant type
        if(grant_type != "client_credentials")
        {
            return BadRequest(new
            {
                error = "Unsupported grant type"
            });
        }
        
        //Check if authorization header exists
        if(!Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            return Unauthorized(new
            {
                error = "Missing Authorization header"
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

        //Authenticate the client using the bank client service
        var result = await _bankClientService.AuthenticateOAuthClientAsync(clientId, clientSecret, scope);
        if (!result.Success)
        {
            return result.Error switch
            {
                OAuthErrorEnum.InvalidClient => Unauthorized(new
                {
                    error = "Invalid client credentials"
                }),
                OAuthErrorEnum.InvalidScope => BadRequest(new
                {
                    error = "Invalid Scope"
                }),
                OAuthErrorEnum.TemporarilyUnavailable => StatusCode(503, new
                {
                    error = "Database Error occurred. Please try again"
                }),
                _ => StatusCode(500, new
                {
                    error = "Server Error"
                })
            };
        }
        return Ok(new
        {
            access_token = result.AccessToken,
            token_type = "Bearer",
            expires_in = result.ExpiresIn,
            scope = result.Scope
        });
    }
}