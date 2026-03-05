using StudentPayments_API.Models.Enums;
using StudentPayments_API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class PaymentTransaction
{
    [Key]
    public int TransactionId {get;set;}
    [Required]
    public int BankClientId {get;set;}
    [Required, MaxLength(50)]
    public string BankReference {get;set;}
    [Required]
    public decimal Amount {get;set;}
    [Required]
    public PaymentTransactionStatusEnum Status {get;set;}
    public DateTime CreatedAt {get;set;}
    [Required]
    public PaymentTypeEnum PaymentType {get;set;}
    [Required]
    public PaymentChannelEnum PaymentChannel {get;set;}
    [Required]
    public int StudentId {get;set;}
    [Required]
    public Guid InternalReference {get;set;}
    [Required]
    public CurrencyEnum CurrencyType {get;set;}
    [Required]
    public int IdempotencyKeyId {get;set;}
    public BankClient BankClient {get;set;}
    public Student Student {get;set;}
    public IdempotencyKey IdempotencyKey {get;set;}
}