namespace StudentPayments_API.Models.Enums
{
    public enum OAuthErrorEnum
    {
        None,
        InvalidClient,
        InvalidScope,
        TemporarilyUnavailable,
        ServerError,
        NotFound, 
        UnsupportedGrantType,
        Inactive,
        Conflict,
        InvalidRequest
    }
}