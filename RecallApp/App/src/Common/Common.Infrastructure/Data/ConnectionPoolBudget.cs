using Npgsql;

namespace Common.Infrastructure.Data;

/// <summary>
/// Представляет бюджет соединений общего пула: распределяет квоту между модулями при старте.
/// </summary>
/// <remarks>
/// На каждую базу данных нужен отдельный экземпляр.
/// </remarks>
public sealed class ConnectionPoolBudget
{
    private readonly object _gate = new();
    private int _poolsInUse;

    /// <summary>
    /// Инициализирует бюджет соединений общего пула.
    /// </summary>
    /// <param name="dataSource">Общий источник соединений.</param>
    public ConnectionPoolBudget(NpgsqlDataSource dataSource)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        MaximumPoolSize = new NpgsqlConnectionStringBuilder(dataSource.ConnectionString).MaxPoolSize;
    }

    /// <summary>
    /// Возвращает общий размер пула соединений.
    /// </summary>
    public int MaximumPoolSize { get; }

    /// <summary>
    /// Возвращает число уже зарезервированных соединений.
    /// </summary>
    public int PoolsInUse
    {
        get { lock (_gate) return _poolsInUse; }
    }

    /// <summary>
    /// Резервирует заданное число соединений из общего пула.
    /// </summary>
    /// <param name="amount">Число резервируемых соединений.</param>
    /// <returns>Аренда, возвращающая квоту в бюджет при освобождении.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Число резервируемых соединений меньше либо равно нулю.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Резервирование превышает доступный остаток пула.
    /// </exception>
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
    /// Возвращает зарезервированные соединения в бюджет.
    /// </summary>
    /// <param name="amount">Число возвращаемых соединений.</param>
    private void Release(int amount)
    {
        lock (_gate) _poolsInUse -= amount;
    }

    /// <summary>
    /// Возвращает зарезервированную квоту в бюджет при освобождении.
    /// </summary>
    private sealed class Lease(ConnectionPoolBudget budget, int amount) : IDisposable
    {
        private int _remaining = amount;

        /// <summary>
        /// Освобождает аренду и возвращает квоту в бюджет.
        /// </summary>
        /// <remarks>
        /// Срабатывает один раз: повторное освобождение ничего не меняет.
        /// </remarks>
        public void Dispose()
        {
            var released = Interlocked.Exchange(ref _remaining, 0);
            if (released > 0) budget.Release(released);
        }
    }
}
