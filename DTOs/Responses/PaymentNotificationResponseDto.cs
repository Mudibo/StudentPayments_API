using System;
using StudentPayments_API.Models.Enums;
namespace StudentPayments_API.DTOs.Responses;

public class PaymentNotificationResponseDto
{
    public bool Success { get; set; }
    public string Error { get; set; }
    public string Message { get; set; }
    public Guid? TransactionUuid { get; set; }
    public string Status { get; set; }
}