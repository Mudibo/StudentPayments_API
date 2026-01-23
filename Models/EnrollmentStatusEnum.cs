using NpgsqlTypes;

namespace StudentPayments_API.Models
{
    public enum EnrollmentStatusEnum
    {
        [PgName("Active")]
        Active,
        [PgName("Inactive")]
        Inactive
    }
}