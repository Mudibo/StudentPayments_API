using NpgsqlTypes;
using System.Runtime.Serialization;
namespace StudentPayments_API.Models.Enums;

public enum PaymentTransactionStatusEnum
{
    [EnumMember(Value = "SUCCESS")]
    SUCCESS,
    [EnumMember(Value = "FAILED")]
    FAILED,
}