namespace Sets.Infrastructure.Data.Outbox;

/// <summary>
/// Настройки outbox-воркера (секция <c>Sets:Outbox</c>).
/// </summary>
internal sealed class OutboxOptions
{
    /// <summary>Выключает фоновую доставку: нужно тестам, где воркер запускается вручную.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Как часто воркер проверяет очередь.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>Сколько сообщений брать за один заход в полосе.</summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>После скольких неудач сообщение переходит в dead.</summary>
    public int MaxAttempts { get; set; } = 10;

    /// <summary>Срок аренды: столько воркер считается живым без продления; истёкшую перехватывает другой.</summary>
    public TimeSpan LeaseDuration { get; set; } = TimeSpan.FromSeconds(60);
}
