namespace StudentPayments_API.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using StudentPayments_API.Models.Enums;

public class BankClient
{
    [Key]
    public int BankClientId { get; set; }
    [Required, MaxLength(50)]
    public string ClientId { get; set; }
    [Required, MaxLength(255)]
    public string ClientSecretHash { get; set; }
    [Required, MaxLength(100)]
    public string BankName { get; set; }
    [Required]
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    [MaxLength(255)]
    public string AllowedScopes { get; set; }

    [NotMapped]
    public List<string> AllowedScopeList
    {
        get => string.IsNullOrEmpty(AllowedScopes)
            ? new List<string>()
            : AllowedScopes.Split(',')
                .Select(s => s.Trim())
                .ToList();
        set => AllowedScopes = string.Join(",", value.Select(e => e.ToString()));
    }
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; }
    public ICollection<IdempotencyKey> IdempotencyKeys { get; set; }

}