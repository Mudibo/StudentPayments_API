namespace StudentPayments_API.DTOs.Requests;

public class OAuthClientAuthRequestDto
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Scope { get; set; }
    public string GrantType { get; set; }
}