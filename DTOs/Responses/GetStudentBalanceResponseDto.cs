using StudentPayments_API.Models.Enums;

namespace StudentPayments_API.DTOs.Responses;

public class GetStudentBalanceResponseDto
{
    public string AdmissionNumber { get; set; }
    public decimal TotalDues { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal Balance { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
}