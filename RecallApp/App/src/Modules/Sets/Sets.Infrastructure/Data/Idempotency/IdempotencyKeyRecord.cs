namespace Sets.Infrastructure.Data.Idempotency;

/// <summary>
/// Строка ключа идемпотентности модуля Sets (таблица <c>sets.idempotency_keys</c>).
/// </summary>
/// <remarks>
/// Ключ уникален в паре с владельцем: один и тот же заголовок от разных пользователей - разные ключи.
/// Завершённая строка хранит ответ, который возвращается при повторе до срока истечения.
/// </remarks>
internal sealed class IdempotencyKeyRecord
{
    public Guid OwnerId { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Operation { get; set; } = string.Empty;

    public string RequestHash { get; set; } = string.Empty;

    /// <summary>Статус сохранённого ответа; null - ключ ещё не завершён (в нормальных условиях не сохраняется).</summary>
    public int? StatusCode { get; set; }

    /// <summary>JSON сохранённого ответа для повтора.</summary>
    public string? Response { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
