namespace Api;

/// <summary>
/// Держит аренду квоты пула соединений на время жизни хоста.
/// </summary>
/// <remarks>
/// Обёртка скрывает <see cref="IDisposable"/>: аренду нельзя освободить раньше времени,
/// освобождает её только контейнер при остановке хоста.
/// </remarks>
internal sealed class PoolLeaseHolder(IDisposable lease) : IDisposable
{
    /// <summary>
    /// Освобождает аренду и возвращает квоту в бюджет пула.
    /// </summary>
    public void Dispose() => lease.Dispose();
}
