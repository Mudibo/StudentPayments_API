using Npgsql;
using NpgsqlTypes;
using System.Runtime.Serialization;
namespace StudentPayments_API.Models.Enums;

public enum PaymentChannelEnum
{
    [EnumMember(Value = "Mobile Banking")]
    MobileBanking,
    [EnumMember(Value = "Internet Banking")]
    InternetBanking
}