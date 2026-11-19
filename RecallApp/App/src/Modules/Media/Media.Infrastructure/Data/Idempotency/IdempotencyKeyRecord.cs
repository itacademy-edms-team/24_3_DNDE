namespace Media.Infrastructure.Data.Idempotency;

/// <summary>
/// Запись хранилища идемпотентности модуля Media: один ключ на пару (владелец, ключ).
/// </summary>
/// <remarks>
/// Один и тот же заголовок Idempotency-Key от разных владельцев - разные записи. Завершённая
/// запись хранит ответ, который возвращается при повторе.
/// </remarks>
internal sealed class IdempotencyKeyRecord
{
    /// <summary>Возвращает или задает владельца ключа - вместе с <see cref="Key"/> образует
    /// уникальную пару.</summary>
    public Guid OwnerId { get; set; }

    /// <summary>Возвращает или задает значение заголовка Idempotency-Key.</summary>
    public string Key { get; set; } = null!;

    /// <summary>Возвращает или задает идентификатор операции.</summary>
    public string Operation { get; set; } = null!;

    /// <summary>Возвращает или задает хеш канонической формы запроса: отличает повтор с тем же
    /// телом от конфликта ключа.</summary>
    public string RequestHash { get; set; } = null!;

    /// <summary>Возвращает или задает статус сохранённого ответа; <see langword="null"/>, пока ключ
    /// не завершён.</summary>
    public int? StatusCode { get; set; }

    /// <summary>Возвращает или задает JSON сохранённого ответа для повтора.</summary>
    public string? Response { get; set; }

    /// <summary>Возвращает или задает состояние долгой операции (<c>in_progress</c> /
    /// <c>completed</c>); <see langword="null"/> в режиме A.</summary>
    public string? Status { get; set; }

    /// <summary>Возвращает или задает владельца аренды: защищает от второго обработчика на том же
    /// ключе.</summary>
    public Guid? LeaseOwner { get; set; }

    /// <summary>Возвращает или задает момент, до которого аренда жива; после истечения работу
    /// перехватывает следующий запрос.</summary>
    public DateTime? LeaseUntil { get; set; }

    /// <summary>Возвращает или задает момент истечения ключа: после него ключ не используется
    /// и удаляется чистильщиком.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Возвращает или задает момент создания ключа.</summary>
    public DateTime CreatedAt { get; set; }
}
