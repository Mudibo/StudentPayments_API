namespace StudentPayments_API.Models;

public enum StudentValidationStatus
{
    Valid,
    NotFound,
    Inactive,
    Error,
    TransientError
}