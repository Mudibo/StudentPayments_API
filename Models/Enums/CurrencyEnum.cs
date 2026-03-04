using NpgsqlTypes;
using System.Runtime.Serialization;
namespace StudentPayments_API.Models.Enums;
public enum CurrencyEnum
{
    [EnumMember(Value = "KES")]
    KES,
    [EnumMember(Value = "USD")]
    USD,
    [EnumMember(Value = "EUR")]
    EUR,
    [EnumMember(Value = "GBP")]
    GBP
}