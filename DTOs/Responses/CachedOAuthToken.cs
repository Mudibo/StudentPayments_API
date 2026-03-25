namespace StudentPayments_API.DTOs.Responses;

public class CachedOAuthToken
{
    public string AccessToken { get; set; }
    public string TokenType { get; set; }
    public int ExpiresIn { get; set; }
    public string Scope { get; set; }
    public int BankClientId { get; set; }
}