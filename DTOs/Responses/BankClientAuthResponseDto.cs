using StudentPayments_API.Models.Enums;

namespace StudentPayments_API.DTOs.Responses;
public class BankClientAuthResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public BankAuthErrorEnum ErrorEnum { get; set; }
    public string AccessToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string BankName { get; set; }
}