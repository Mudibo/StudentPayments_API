namespace StudentPayments_API.Models;
using NpgsqlTypes;
public enum ProgramEnum
{
    [PgName("Computer Science")]
    ComputerScience,
    [PgName("Law")]
    Law,
    [PgName("International Relations")]
    InternationalRelations
}