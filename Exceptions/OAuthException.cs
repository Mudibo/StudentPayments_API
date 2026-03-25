using StudentPayments_API.Models.Enums;

namespace StudentPayments_API.DTOs.Responses;
public class OAuthException : Exception
{
    public string Error { get; }
    public string ErrorDescription { get; }
    public OAuthException(OAuthErrorEnum error, string description) : base(description)
    {
        Error = error.ToOAuthErrorString();
        ErrorDescription = description;
    }
}