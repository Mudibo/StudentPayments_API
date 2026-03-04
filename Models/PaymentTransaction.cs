using StudentPayments_API.Models.Enums;
using StudentPayments_API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class PaymentTransaction
{
    [Key]
    public int TransactionId {get;set;}
    public int BankClientId {get;set;}
    public string BankReference {get;set;}
    public decimal Amount {get;set;}
    public PaymentTransactionStatusEnum Status {get;set;}
    public DateTime CreatedAt {get;set;}
    public PaymentTypeEnum PaymentType {get;set;}
    public PaymentChannelEnum PaymentChannel {get;set;}
    public int StudentId {get;set;}
    public Guid InternalReference {get;set;}
    public CurrencyEnum CurrencyType {get;set;}
    public int IdempotencyKeyId {get;set;}
    public BankClient BankClient {get;set;}
    public Student Student {get;set;}
    public IdempotencyKey IdempotencyKey {get;set;}
}