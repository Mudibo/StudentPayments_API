namespace StudentPayments_API.Models.Enums;

public static class OAuthScopeEnum
{
    public const string StudentValidate = "student.validate";
    public const string PaymentNotification = "payment.notification";
    public static readonly string[] All = {StudentValidate, PaymentNotification};
}