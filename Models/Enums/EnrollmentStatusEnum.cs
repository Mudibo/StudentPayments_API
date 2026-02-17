using NpgsqlTypes;

namespace StudentPayments_API.Models.Enums;
    public enum EnrollmentStatusEnum
    {
        [PgName("Active")]
        Active,
        [PgName("Inactive")]
        Inactive
    }
