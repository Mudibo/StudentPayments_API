using NpgsqlTypes;
using System.Runtime.Serialization;
namespace StudentPayments_API.Models.Enums;
public enum EnrollmentStatusEnum
{
    [EnumMember(Value = "Active")]
    Active,
    [EnumMember(Value = "Inactive")]
    Inactive
}
