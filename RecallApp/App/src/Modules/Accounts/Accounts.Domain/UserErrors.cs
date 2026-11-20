using ErrorOr;

namespace Accounts.Domain;

/// <summary>
/// Предоставляет каталог ошибок домена пользователя.
/// </summary>
/// <remarks>
/// Код ошибки является частью контракта для клиента (ADR 0038) и имеет формат
/// <c>&lt;Сущность&gt;.&lt;Правило&gt;</c>: по нему выбирается текст для пользователя.
/// Описание ошибки предназначено для логов и в интерфейс не попадает.
/// Метаданные используются в логах и внутренних обработчиках; в ответ ProblemDetails
/// сейчас уходит только <c>target</c>.
/// </remarks>
public static class UserErrors
{
    /// <summary>Claim sub пуст или не является UUID: идентичность неполная, доменных данных нет.</summary>
    public static readonly Error KeycloakSubInvalid = Error.Validation(
        code: "User.KeycloakSubInvalid",
        description: "KeycloakSub must be a non-empty UUID.",
        metadata: new Dictionary<string, object> { ["target"] = "user.keycloakSub" });

    /// <summary>Email не задан.</summary>
    public static readonly Error EmailRequired = Error.Validation(
        code: "User.EmailRequired",
        description: "Email must not be empty.",
        metadata: new Dictionary<string, object> { ["target"] = "user.email" });

    /// <summary>Отображаемое имя не задано или состоит из пробелов.</summary>
    public static readonly Error DisplayNameRequired = Error.Validation(
        code: "User.DisplayNameRequired",
        description: "DisplayName must not be empty after trim.",
        metadata: new Dictionary<string, object> { ["target"] = "user.displayName" });

    /// <summary>Создаёт ошибку превышения допустимой длины отображаемого имени.</summary>
    /// <param name="length">Фактическая длина после нормализации.</param>
    /// <returns>Ошибка валидации с кодом <c>User.DisplayNameTooLong</c> и фактической
    /// и максимальной длиной в метаданных.</returns>
    public static Error DisplayNameTooLong(int length) => Error.Validation(
        code: "User.DisplayNameTooLong",
        description: $"DisplayName length {length} exceeds limit {UserConstraints.DisplayNameMaxLength}.",
        metadata: new Dictionary<string, object>
        {
            ["target"] = "user.displayName",
            ["length"] = length,
            ["max"] = UserConstraints.DisplayNameMaxLength,
        });

    /// <summary>Строки пользователя нет: JIT-провижининг не выполнен.</summary>
    /// <remarks>Провижининг выполняется при первом доменном запросе (ADR 0020).</remarks>
    public static readonly Error NotFound = Error.NotFound(
        code: "User.NotFound",
        description: "Domain user is not provisioned for this keycloak sub.");
}
