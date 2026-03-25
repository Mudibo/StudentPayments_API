namespace StudentPayments_API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using StudentPayments_API.Models.Enums;

public class IdempotencyKey
{
    [Key]
    public int Id { get; set; }
    [Required]
    public int BankClientId { get; set; }
    [Required, MaxLength(100)]
    public string Key { get; set; }
    [Required, MaxLength(255)]
    public string RequestHash { get; set; }
    [Required]
    public IdempotencyResourceTypeEnum ResourceType { get; set; }
    public DateTime CreatedAt { get; set; }
    public BankClient BankClient { get; set; }
    public PaymentTransaction PaymentTransaction { get; set; }
}
