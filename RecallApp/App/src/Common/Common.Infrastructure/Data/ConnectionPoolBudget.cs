using Npgsql;

namespace Common.Infrastructure.Data;

/// <summary>
/// Бюджет соединений общего NpgsqlDataSource: распределяет пул между модулями при старте.
/// На каждую БД необходимо создавать новый экземляр.
/// </summary>
public sealed class ConnectionPoolBudget
{
    private readonly object _gate = new();
    private int _poolsInUse;

    public ConnectionPoolBudget(NpgsqlDataSource dataSource)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        MaximumPoolSize = new NpgsqlConnectionStringBuilder(dataSource.ConnectionString).MaxPoolSize;
    }

    public int MaximumPoolSize { get; }

    public int PoolsInUse
    {
        get { lock (_gate) return _poolsInUse; }
    }

    /// <summary>
    /// Резервирует <paramref name="amount"/> соединений из общего пула.
    /// </summary>
    /// <param name="amount">Число резервируемых соединений.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">Число резервируемых соединений меньше либо равно 0</exception>
    /// <exception cref="InvalidOperationException">Число резервируемых соединений превышает число число доступных к резервированию.</exception>
    public IDisposable GetPools(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be a positive number of connections.");

        lock (_gate)
        {
            if (_poolsInUse + amount > MaximumPoolSize)
                throw new InvalidOperationException(
                    $"Cannot reserve {amount} connection(s): {_poolsInUse} of {MaximumPoolSize} pool connections are already reserved by modules.");

            _poolsInUse += amount;
            return new Lease(this, amount);
        }
    }

    /// <summary>
    /// Убирает зарезервированные соединения в резерв.  
    /// </summary>
    /// <param name="amount"></param>
    private void Release(int amount)
    {
        lock (_gate) _poolsInUse -= amount;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="budget"></param>
    /// <param name="amount"></param>
    /// <summary>Возвращает зарезервированную квоту в бюджет при Dispose (однократно).</summary>
    private sealed class Lease(ConnectionPoolBudget budget, int amount) : IDisposable
    {
        private int _remaining = amount;

        public void Dispose()
        {
            var released = Interlocked.Exchange(ref _remaining, 0);
            if (released > 0) budget.Release(released);
        }
    }
}
