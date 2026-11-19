namespace Media.Infrastructure.Data.Idempotency;

/// <summary>
/// Запись хранилища идемпотентности модуля Media: один ключ на пару (владелец, ключ).
/// </summary>
/// <remarks>
/// Один и тот же заголовок Idempotency-Key от разных владельцев - разные записи.
/// Завершённая запись хранит ответ, который возвращается при повторе и живёт до истечения.
/// </remarks>
internal sealed class IdempotencyKeyRecord
{
    /// <summary>Владелец ключа.</summary>
    /// <remarks>
    /// Вместе с <see cref="Key"/> образует уникальную пару.
    /// </remarks>
    public Guid OwnerId { get; set; }

    /// <summary>Значение заголовка Idempotency-Key.</summary>
    public string Key { get; set; } = null!;

    /// <summary>Идентификатор операции.</summary>
    /// <remarks>Значение стабильно, потому что уже записано в базе.</remarks>
    public string Operation { get; set; } = null!;

    /// <summary>Хеш канонической формы запроса: отличает повтор с тем же телом от конфликта ключа.</summary>
    public string RequestHash { get; set; } = null!;

    /// <summary>Статус сохранённого ответа; <see langword="null"/>, пока ключ не завершён.</summary>
    public int? StatusCode { get; set; }

    /// <summary>JSON сохранённого ответа для повтора; <see langword="null"/>, пока операция не завершена.</summary>
    public string? Response { get; set; }

    /// <summary>
    /// Состояние долгой операции (<c>in_progress</c> / <c>completed</c>);
    /// <see langword="null"/> в режиме A, где промежуточного состояния нет.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>Кто сейчас выполняет операцию.</summary>
    /// <remarks>Защищает от второго обработчика на том же ключе.</remarks>
    public Guid? LeaseOwner { get; set; }

    /// <summary>До какого момента в UTC аренда жива; после истечения работу перехватывает следующий запрос.</summary>
    public DateTime? LeaseUntil { get; set; }

    /// <summary>Момент истечения в UTC: после него ключ не используется и удаляется чистильщиком.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Момент создания ключа в UTC.</summary>
    public DateTime CreatedAt { get; set; }
}
