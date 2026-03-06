using FinanceTrack.Finance.Core.CategoryAggregate;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.RecurringTransactionAggregate;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.Infrastructure.Data;

namespace FinanceTrack.Finance.IntegrationTests.Data;

public abstract class BaseEfRepoTestFixture
{
    protected AppDbContext _dbContext;

    protected BaseEfRepoTestFixture()
    {
        var options = CreateNewContextOptions();
        var fakeEventDispatcher = Substitute.For<IDomainEventDispatcher>();

        _dbContext = new AppDbContext(options, fakeEventDispatcher);
    }

    protected static DbContextOptions<AppDbContext> CreateNewContextOptions()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();

        var builder = new DbContextOptionsBuilder<AppDbContext>();
        builder
            .UseInMemoryDatabase("ft-int-tests-" + Guid.NewGuid().ToString())
            .UseInternalServiceProvider(serviceProvider);

        return builder.Options;
    }

    protected EfRepository<FinancialTransaction> GetFinancialTransactionRepository()
    {
        return new EfRepository<FinancialTransaction>(_dbContext);
    }

    protected EfRepository<Wallet> GetWalletRepository()
    {
        return new EfRepository<Wallet>(_dbContext);
    }

    protected EfRepository<Category> GetCategoryRepository()
    {
        return new EfRepository<Category>(_dbContext);
    }

    protected EfRepository<RecurringTransaction> GetRecurringTransactionRepository()
    {
        return new EfRepository<RecurringTransaction>(_dbContext);
    }

    protected Task SaveChangesAsync(CancellationToken ct = default) =>
        _dbContext.SaveChangesAsync(ct);
}
