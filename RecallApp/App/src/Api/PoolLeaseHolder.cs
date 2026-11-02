namespace Api;

/// <summary>
/// Держит lease'ы квот пула соединений на время жизни хоста.
/// Sealed-обёртка скрывает IDisposable от случайного инжекта и раннего Dispose:
/// Dispose вызывается только контейнером при остановке хоста.
/// </summary>
internal sealed class PoolLeaseHolder(IDisposable lease) : IDisposable
{
    public void Dispose() => lease.Dispose();
}
