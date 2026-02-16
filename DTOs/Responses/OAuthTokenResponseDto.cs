
using StudentPayments_API.Models.Enums;

public class OAuthTokenResponseDto
{
    public string access_token {get;set;}
    public string token_type {get;set;}
    public int expires_in {get;set;}
    public string scope {get;set;}
}