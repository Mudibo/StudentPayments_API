using System;
using StudentPayments_API.Models.Enums;
namespace StudentPayments_API.DTOs.Responses;
public class GetStudentPaymentNotificationResponseDto
{
    public Guid InternalReference {get;set;}
    public decimal Amount {get;set;}
    public PaymentTransactionStatusEnum Status {get;set;}
    public PaymentTypeEnum PaymentType {get;set;}
    public PaymentChannelEnum PaymentChannel {get;set;}
    public DateTime CreatedAt {get;set;}
    public string BankReference {get;set;}
    public CurrencyEnum CurrencyType {get;set;}
}