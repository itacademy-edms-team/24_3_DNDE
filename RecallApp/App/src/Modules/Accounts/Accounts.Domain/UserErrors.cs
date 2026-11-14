using ErrorOr;

namespace Accounts.Domain;

/// <summary>
/// Каталог ошибок домена Accounts. Код - часть контракта для клиента (ADR 0038):
/// формат <c>&lt;Сущность&gt;.&lt;Правило&gt;</c>, по нему клиент понимает, что вывести клиенту.
/// </summary>
public static class UserErrors
{
    /// <summary>Claim sub пуст или не является UUID: идентичность неполная, доменных данных нет.</summary>
    public static readonly Error KeycloakSubInvalid = Error.Validation(
        code: "User.KeycloakSubInvalid",
        description: "KeycloakSub must be a non-empty UUID.",
        metadata: new Dictionary<string, object> { ["target"] = "user.keycloakSub" });

    public static readonly Error EmailRequired = Error.Validation(
        code: "User.EmailRequired",
        description: "Email must not be empty.",
        metadata: new Dictionary<string, object> { ["target"] = "user.email" });

    public static readonly Error DisplayNameRequired = Error.Validation(
        code: "User.DisplayNameRequired",
        description: "DisplayName must not be empty after trim.",
        metadata: new Dictionary<string, object> { ["target"] = "user.displayName" });

    public static Error DisplayNameTooLong(int length) => Error.Validation(
        code: "User.DisplayNameTooLong",
        description: $"DisplayName length {length} exceeds limit {User.DisplayNameMaxLength}.",
        metadata: new Dictionary<string, object>
        {
            ["target"] = "user.displayName",
            ["max"] = User.DisplayNameMaxLength,
        });

    /// <summary>Строки пользователя нет: JIT-провижининг не выполнен (ADR 0020).</summary>
    public static readonly Error NotFound = Error.NotFound(
        code: "User.NotFound",
        description: "Domain user is not provisioned for this keycloak sub.");
}
