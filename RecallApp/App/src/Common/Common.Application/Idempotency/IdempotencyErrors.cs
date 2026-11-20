using ErrorOr;

namespace Common.Application.Idempotency;

/// <summary>
/// Предоставляет каталог ошибок идемпотентности.
/// </summary>
/// <remarks>
/// Коды - часть контракта клиента (ADR 0038): <c>Idempotency.&lt;Правило&gt;</c>.
/// </remarks>
public static class IdempotencyErrors
{
    /// <summary>
    /// Ключ идемпотентности не задан.
    /// </summary>
    public static readonly Error KeyRequired = Error.Validation(
        code: "Idempotency.KeyRequired",
        description: "Idempotency-Key header is required for this operation.",
        metadata: new Dictionary<string, object> { ["target"] = "header.Idempotency-Key" });

    /// <summary>
    /// Ключ идемпотентности не является UUID.
    /// </summary>
    public static readonly Error KeyInvalid = Error.Validation(
        code: "Idempotency.KeyInvalid",
        description: "Idempotency-Key must be a UUID.",
        metadata: new Dictionary<string, object> { ["target"] = "header.Idempotency-Key" });

    /// <summary>
    /// Создаёт ошибку повторного использования ключа с другим телом.
    /// </summary>
    /// <param name="key">Значение заголовка Idempotency-Key.</param>
    /// <returns>Ошибка конфликта с кодом <c>Idempotency.KeyReused</c>.</returns>
    public static Error KeyReused(string key) => Error.Conflict(
        code: "Idempotency.KeyReused",
        description: $"Idempotency-Key '{key}' was already used with a different request body.",
        metadata: new Dictionary<string, object> { ["target"] = "header.Idempotency-Key" });

    /// <summary>
    /// Создаёт ошибку занятости ключа незавершённой операцией.
    /// </summary>
    /// <remarks>
    /// Штатно не возникает в режиме коротких операций: ключ и ответ коммитятся вместе.
    /// </remarks>
    /// <param name="key">Значение заголовка Idempotency-Key.</param>
    /// <returns>Ошибка конфликта с кодом <c>Idempotency.InProgress</c>.</returns>
    public static Error InProgress(string key) => Error.Conflict(
        code: "Idempotency.InProgress",
        description: $"Idempotency-Key '{key}' is reserved by an operation that has not completed.",
        metadata: new Dictionary<string, object> { ["target"] = "header.Idempotency-Key" });
}
