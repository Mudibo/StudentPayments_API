namespace StudentPayments_API.Models.Enums;

public enum BankAuthErrorEnum
{
    None = 0,
    MissingCredentials,
    InvalidCredentials,
    ClientInactive,
    DatabaseError,
    UnexpectedError
}