using Npgsql;
using NpgsqlTypes;

namespace StudentPayments_API.Models;

public enum PaymentTypeEnum
{
    [PgName("cash")]
    Cash,
    [PgName("check")]
    Check,
    [PgName("pesalink")]
    Pesalink
}