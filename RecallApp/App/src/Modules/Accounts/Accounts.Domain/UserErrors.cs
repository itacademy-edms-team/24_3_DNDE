using ErrorOr;

namespace Accounts.Domain;

/// <summary>
/// Каталог ошибок домена Accounts.
/// </summary>
public static class UserErrors
{
    public static readonly Error KeycloakSubEmpty = Error.Validation(
        code: "InvalidKeycloakSub",
        description: "KeycloakSub must not be empty.",
        metadata: new Dictionary<string, object> { ["target"] = "user.keycloakSub" });

    public static readonly Error EmailEmpty = Error.Validation(
        code: "InvalidEmail",
        description: "Email must not be empty.",
        metadata: new Dictionary<string, object> { ["target"] = "user.email" });

    public static Error DisplayNameInvalid(int maxLength) => Error.Validation(
        code: "InvalidDisplayName",
        description: $"DisplayName must be 1..{maxLength} characters after trim.",
        metadata: new Dictionary<string, object> { ["target"] = "user.displayName" });
}
