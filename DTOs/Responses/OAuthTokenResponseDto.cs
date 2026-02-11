
using StudentPayments_API.Models.Enums;

public class OAuthTokenResponseDto
{
    public bool Success { get; set; }
    public OAuthErrorEnum Error { get; set; }
    public string AccessToken { get; set; }
    public int ExpiresIn { get; set; }
    public string Scope { get; set; }
}