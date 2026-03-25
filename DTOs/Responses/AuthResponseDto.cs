namespace StudentPayments_API.DTOs.Responses;

public class AuthResponseDto
{
    public string access_token { get; set; }
    public bool success { get; set; }
    public string token_type { get; set; }
    public string error { get; set; }
    public string role { get; set; }
    public int? expires_in { get; set; }
}