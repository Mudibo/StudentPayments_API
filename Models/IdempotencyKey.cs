namespace StudentPayments_API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using StudentPayments_API.Models.Enums;

[Table("idempotency_keys")]
public class IdempotencyKey
{
    [Key]
    [Column("id")]
    public int Id {get;set;}

    [Column("bank_client_id")]
    public int BankClientId {get;set;}

    [Column("idempotency_key")]
    public string Key {get;set;}

    [Column("request_hash")]
    public string RequestHash {get;set;}

    [Column("resource_type")]
    public IdempotencyResourceTypeEnum ResourceType {get;set;}
    
    [Column("created_at")]
    public DateTime CreatedAt {get;set;}

    public BankClient BankClient {get;set;}
    public ICollection<PaymentTransaction> PaymentTransactions {get;set;}
}
