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

    /// <summary>Claim sub не является UUID: идентичность неполная, доменных данных нет.</summary>
    public static readonly Error KeycloakSubInvalid = Error.Validation(
        code: "InvalidKeycloakSub",
        description: "KeycloakSub must be a UUID.",
        metadata: new Dictionary<string, object> { ["target"] = "user.keycloakSub" });

    /// <summary>Строки пользователя нет: JIT-провижининг не выполнен (ADR 0020).</summary>
    public static readonly Error NotFound = Error.NotFound(
        code: "UserNotFound",
        description: "Domain user is not provisioned for this keycloak sub.");

    public static Error DisplayNameInvalid(int maxLength) => Error.Validation(
        code: "InvalidDisplayName",
        description: $"DisplayName must be 1..{maxLength} characters after trim.",
        metadata: new Dictionary<string, object> { ["target"] = "user.displayName" });
}
