using Npgsql;
using NpgsqlTypes;
using System.Runtime.Serialization;
namespace StudentPayments_API.Models.Enums;

public enum PaymentTypeEnum
{
    [EnumMember(Value = "cash")]
    Cash,
    [EnumMember(Value = "check")]
    Check,
    [EnumMember(Value = "pesalink")]
    Pesalink
}