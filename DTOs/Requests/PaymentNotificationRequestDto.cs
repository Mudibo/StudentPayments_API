using System.ComponentModel.DataAnnotations;
using StudentPayments_API.Models.Enums;

namespace StudentPayments_API.DTOs.Requests;

public class PaymentNotificationRequestDto
{
    [Required]
    public string AdmissionNumber { get; set; }
    [Required]
    public decimal Amount { get; set; }
    [Required]
    public CurrencyEnum Currency { get; set; }
    [Required]
    public PaymentTypeEnum PaymentType { get; set; }
    [Required]
    public PaymentChannelEnum PaymentChannel { get; set; }
    [Required]
    public string BankReference { get; set; }
    [Required]
    public PaymentTransactionStatusEnum Status { get; set; }
}