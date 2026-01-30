using System;

namespace StudentPayments_API.DTOs.Responses;

public class PaymentResponseDto
{
    public int StudentId { get; set; }
    public string ReferenceNumber { get; set; }
    public DateTime PaymentDateTime { get; set; }
    public string PaymentType { get; set; }
    public decimal Amount { get; set; }
    public string PaymentChannel { get; set; }
}