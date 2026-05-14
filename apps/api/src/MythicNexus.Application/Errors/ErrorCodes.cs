namespace MythicNexus.Application.Errors;

public static class ErrorCodes
{
    public const string ValidationFailed = "validation.failed";

    public const string AuthInvalidCredentials = "auth.invalid_credentials";
    public const string AuthEmailAlreadyExists = "auth.email_already_exists";
    public const string AuthUsernameTaken = "auth.username_taken";
    public const string AuthRegistrationConflict = "auth.registration_conflict";
    public const string AuthEmailNotConfirmed = "auth.email_not_confirmed";
    public const string AuthAccountLocked = "auth.account_locked";
    public const string AuthInvalidOrExpiredToken = "auth.invalid_or_expired_token";

    public const string UserNotFound = "user.not_found";

    public const string InternalServerError = "internal.server_error";
}
