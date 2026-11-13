namespace Media.Infrastructure.Data.Idempotency;

/// <summary>
/// Запись журнала идемпотентности модуля Media: один ключ на пару (владелец, операция).
/// </summary>
internal sealed class IdempotencyKeyRecord
{
    public Guid OwnerId { get; set; }

    public string Key { get; set; } = null!;

    public string Operation { get; set; } = null!;

    public string RequestHash { get; set; } = null!;

    public int? StatusCode { get; set; }

    public string? Response { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
