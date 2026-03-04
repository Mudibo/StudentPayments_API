using System.Runtime.Serialization;
using NpgsqlTypes;

namespace StudentPayments_API.Models.Enums;

public enum IdempotencyResourceTypeEnum
{
    [EnumMember(Value = "payment_transaction")]
    PaymentTransaction
}