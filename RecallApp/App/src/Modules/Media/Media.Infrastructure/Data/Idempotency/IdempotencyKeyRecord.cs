namespace Media.Infrastructure.Data.Idempotency;

/// <summary>
/// Запись хранилища идемпотентности модуля Media: один ключ на пару (владелец, операция).
/// </summary>
internal sealed class IdempotencyKeyRecord
{
    public Guid OwnerId { get; set; }

    public string Key { get; set; } = null!;

    public string Operation { get; set; } = null!;

    public string RequestHash { get; set; } = null!;

    public int? StatusCode { get; set; }

    public string? Response { get; set; }

    /// <summary>
    /// Состояние аренды для долгих операций: `in_progress` / `completed`.
    /// У коротких операций (режим A) остаётся пустым: ключ и результат коммитятся одной транзакцией.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>Кто сейчас выполняет операцию: защищает от второго обработчика на том же ключе.</summary>
    public Guid? LeaseOwner { get; set; }

    /// <summary>До какого момента аренда жива; после истечения работу перехватывает следующий запрос.</summary>
    public DateTime? LeaseUntil { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
