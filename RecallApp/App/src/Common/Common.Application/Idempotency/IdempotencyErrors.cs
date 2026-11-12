using ErrorOr;

namespace Common.Application.Idempotency;

/// <summary>
/// Ошибки идемпотентности. Коды - часть контракта клиента (ADR 0038):
/// <c>Idempotency.&lt;Правило&gt;</c>.
/// </summary>
public static class IdempotencyErrors
{
    public static readonly Error KeyRequired = Error.Validation(
        code: "Idempotency.KeyRequired",
        description: "Idempotency-Key header is required for this operation.",
        metadata: new Dictionary<string, object> { ["target"] = "header.Idempotency-Key" });

    public static readonly Error KeyInvalid = Error.Validation(
        code: "Idempotency.KeyInvalid",
        description: "Idempotency-Key must be a UUID.",
        metadata: new Dictionary<string, object> { ["target"] = "header.Idempotency-Key" });

    /// <summary>Тот же ключ пришёл с другим телом: это ошибка клиента, а не повтор.</summary>
    public static Error KeyReused(string key) => Error.Conflict(
        code: "Idempotency.KeyReused",
        description: $"Idempotency-Key '{key}' was already used with a different request body.",
        metadata: new Dictionary<string, object> { ["target"] = "header.Idempotency-Key" });

    /// <summary>Ключ занят незавершённой операцией. Штатно не возникает: ключ и ответ коммитятся вместе.</summary>
    public static Error InProgress(string key) => Error.Conflict(
        code: "Idempotency.InProgress",
        description: $"Idempotency-Key '{key}' is reserved by an operation that has not completed.",
        metadata: new Dictionary<string, object> { ["target"] = "header.Idempotency-Key" });
}
