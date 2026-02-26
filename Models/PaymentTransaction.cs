using StudentPayments_API.Models.Enums;
using StudentPayments_API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("payment_transactions")]
public class PaymentTransaction
{
    [Key]
    [Column("transaction_id")]
    public int TransactionId {get;set;}

    [Column("bank_client_id")]
    public int BankClientId {get;set;}

    [Column("bank_reference")]
    public string BankReference {get;set;}

    [Column("amount")]
    public decimal Amount {get;set;}

    [Column("status")]
    public PaymentTransactionStatusEnum Status {get;set;}

    [Column("created_at")]
    public DateTime CreatedAt {get;set;}
    
    [Column("payment_type")]
    public PaymentTypeEnum PaymentType {get;set;}

    [Column("payment_channel")]
    public PaymentChannelEnum PaymentChannel {get;set;}

    [Column("student_id")]
    public int StudentId {get;set;}

    [Column("internal_reference")]
    public Guid InternalReference {get;set;}
    
    [Column("currency_type")]
    public CurrencyEnum CurrencyType {get;set;}

    [Column("idempotency_key_id")]
    public int IdempotencyKeyId {get;set;}

    public BankClient BankClient {get;set;}
    public Student Student {get;set;}
    public IdempotencyKey IdempotencyKey {get;set;}
}