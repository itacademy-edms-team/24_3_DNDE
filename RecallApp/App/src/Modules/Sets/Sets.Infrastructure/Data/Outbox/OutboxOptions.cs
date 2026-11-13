namespace Sets.Infrastructure.Data.Outbox;

/// <summary>
/// Настройки диспетчера outbox (секция <c>Sets:Outbox</c>).
/// </summary>
internal sealed class OutboxOptions
{
    /// <summary>Выключает фоновую доставку: нужно тестам, где диспетчер запускается вручную.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Как часто диспетчер проверяет очередь.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>Сколько сообщений брать за один заход.</summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>После скольких неудач сообщение помечается как требующее ручного разбора.</summary>
    public int MaxAttempts { get; set; } = 10;
}
