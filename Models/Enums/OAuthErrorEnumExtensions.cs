
using StudentPayments_API.Models.Enums;

public static class OAuthErrorEnumExtensions
{
    public static string ToOAuthErrorString(this OAuthErrorEnum error)
    {
        return error switch
        {
            OAuthErrorEnum.InvalidClient => "invalid_client",
            OAuthErrorEnum.InvalidScope => "invalid_scope",
            OAuthErrorEnum.TemporarilyUnavailable => "temporarily_unavailable",
            OAuthErrorEnum.ServerError => "server_error",
            OAuthErrorEnum.UnsupportedGrantType => "unsupported_grant_type",
            OAuthErrorEnum.InvalidRequest => "invalid_request",
            OAuthErrorEnum.Inactive => "inactive",
            _ => null
        };
    }
}