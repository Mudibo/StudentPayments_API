namespace StudentPayments_API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using StudentPayments_API.Models.Enums;

public class IdempotencyKey
{
    [Key]
    public int Id {get;set;}
    public int BankClientId {get;set;}
    public string Key {get;set;}
    public string RequestHash {get;set;}
    public IdempotencyResourceTypeEnum ResourceType {get;set;}
    public DateTime CreatedAt {get;set;}
    public BankClient BankClient {get;set;}
    public ICollection<PaymentTransaction> PaymentTransactions {get;set;}
}
