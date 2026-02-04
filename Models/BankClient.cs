namespace StudentPayments_API.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

[Table("bank_clients")]
public class BankClient
{
    [Key]
    [Column("bank_client_id")]
    public int BankClientId {get; set;}
    [Column("client_id")]
    public string ClientId {get; set;}
    [Column("client_secret_hash")]
    public string ClientSecretHash {get;set;}
    [Column("bank_name")]
    public string BankName {get;set;}
    [Column("is_active")]
    public bool IsActive {get;set;}
    [Column("created_at")]
    public DateTime CreatedAt {get;set;}
    public ICollection<Payment> Payments {get;set;}
}