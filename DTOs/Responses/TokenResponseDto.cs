namespace StudentPayments_API.DTOs.Responses;

public class TokenResponseDto //A data transfer object used specifically for returning token data to clients
{
    public string Token {get; set;}
    public DateTime Expiration {get; set;}
    public string Role { get; set; }
}