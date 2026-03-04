namespace StudentPayments_API.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


public class BankClient
{
    [Key]
    public int BankClientId {get; set;}
    public string ClientId {get; set;}
    public string ClientSecretHash {get;set;}
    public string BankName {get;set;}
    public bool IsActive {get;set;}
    public DateTime CreatedAt {get;set;}
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; }
    public ICollection<IdempotencyKey> IdempotencyKeys {get;set;}
    public ICollection<Payment> Payments {get;set;}
}