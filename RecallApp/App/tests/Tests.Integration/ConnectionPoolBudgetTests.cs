using Common.Infrastructure.Data;
using Npgsql;

namespace Tests.Integration;

public sealed class ConnectionPoolBudgetTests
{
    private static ConnectionPoolBudget CreateBudget(int maxPoolSize)
    {
        var dataSource = new NpgsqlDataSourceBuilder($"Host=localhost;Database=unused;Maximum Pool Size={maxPoolSize}").Build();
        return new ConnectionPoolBudget(dataSource);
    }

    [Fact]
    public void GetPools_AmountWithinLimit_ReservesAndReportsInUse()
    {
        var budget = CreateBudget(maxPoolSize: 10);

        using var lease = budget.GetPools(4);

        Assert.Equal(10, budget.MaximumPoolSize);
        Assert.Equal(4, budget.PoolsInUse);
    }

    [Fact]
    public void GetPools_ExhaustedBudget_ThrowsInvalidOperation()
    {
        var budget = CreateBudget(maxPoolSize: 10);
        var first = budget.GetPools(8);

        var exception = Record.Exception(() => budget.GetPools(3));

        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(8, budget.PoolsInUse);
        first.Dispose();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetPools_NonPositiveAmount_ThrowsArgumentOutOfRange(int amount)
    {
        var budget = CreateBudget(maxPoolSize: 10);

        var exception = Record.Exception(() => budget.GetPools(amount));

        Assert.IsType<ArgumentOutOfRangeException>(exception);
        Assert.Equal(0, budget.PoolsInUse);
    }

    [Fact]
    public void Dispose_Lease_ReturnsQuotaToBudget()
    {
        var budget = CreateBudget(maxPoolSize: 10);
        var lease = budget.GetPools(5);

        lease.Dispose();

        Assert.Equal(0, budget.PoolsInUse);
        using var reissued = budget.GetPools(10);
        Assert.Equal(10, budget.PoolsInUse);
    }

    [Fact]
    public void Dispose_LeaseTwice_QuotaReturnedOnce()
    {
        var budget = CreateBudget(maxPoolSize: 10);
        var lease = budget.GetPools(5);

        lease.Dispose();
        lease.Dispose();

        Assert.Equal(0, budget.PoolsInUse);
    }
}
