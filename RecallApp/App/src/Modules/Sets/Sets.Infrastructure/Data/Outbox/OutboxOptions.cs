namespace Sets.Infrastructure.Data.Outbox;

/// <summary>
/// Содержит настройки outbox-воркера (секция <c>Sets:Outbox</c>).
/// </summary>
internal sealed class OutboxOptions
{
    /// <summary>
    /// Возвращает или задает значение, указывающее, включена ли фоновая доставка.
    /// </summary>
    /// <remarks>
    /// В тестах выключается: воркер запускается вручную.
    /// </remarks>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Возвращает или задает интервал, с которым воркер проверяет очередь.
    /// </summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Возвращает или задает число сообщений, берущихся за один заход в полосе.
    /// </summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>
    /// Возвращает или задает число неудач, после которого сообщение переходит в dead.
    /// </summary>
    public int MaxAttempts { get; set; } = 10;

    /// <summary>
    /// Возвращает или задает срок аренды: столько воркер считается живым без продления.
    /// </summary>
    /// <remarks>
    /// Используется slow-полосой; fast-полоса арендует сообщения транзакцией (FOR UPDATE SKIP LOCKED).
    /// </remarks>
    public TimeSpan LeaseDuration { get; set; } = TimeSpan.FromSeconds(60);
}
