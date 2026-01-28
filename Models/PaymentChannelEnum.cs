using Npgsql;
using NpgsqlTypes;

namespace StudentPayments_API.Models;

public enum PaymentChannelEnum
{
    [PgName("Mobile Banking")]
    MobileBanking,
    [PgName("Internet Banking")]
    InternetBanking
}