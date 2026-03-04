namespace StudentPayments_API.Models.Enums;
using NpgsqlTypes;
using System.Runtime.Serialization;
public enum ProgramEnum
{
    [EnumMember(Value = "Computer Science")]
    ComputerScience,
    [EnumMember(Value = "Law")]
    Law,
    [EnumMember(Value = "International Relations")]
    InternationalRelations
}