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
    /// <summary>Возвращает или задает владельца ключа - вместе с <see cref="Key"/> образует
    /// уникальную пару.</summary>
    public Guid OwnerId { get; set; }

    /// <summary>Возвращает или задает значение заголовка Idempotency-Key.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Возвращает или задает идентификатор операции.</summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>Возвращает или задает хеш канонической формы запроса: отличает повтор с тем же
    /// телом от конфликта ключа.</summary>
    public string RequestHash { get; set; } = string.Empty;

    /// <summary>Возвращает или задает статус сохранённого ответа; <see langword="null"/>, пока ключ
    /// не завершён.</summary>
    public int? StatusCode { get; set; }

    /// <summary>Возвращает или задает JSON сохранённого ответа для повтора.</summary>
    public string? Response { get; set; }

    /// <summary>Возвращает или задает момент истечения ключа: после него ключ не используется
    /// и удаляется чистильщиком.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Возвращает или задает момент создания ключа.</summary>
    public DateTime CreatedAt { get; set; }
}
