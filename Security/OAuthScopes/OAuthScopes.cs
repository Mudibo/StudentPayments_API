namespace StudentPayments_API.Security.OAuthScopes;

//Declare OAuth scopes as constants for use throughout the application
public static class OAuthScopes
{
    public const string StudentValidate = "student.validate";
    public static readonly string[] All = { StudentValidate };
}