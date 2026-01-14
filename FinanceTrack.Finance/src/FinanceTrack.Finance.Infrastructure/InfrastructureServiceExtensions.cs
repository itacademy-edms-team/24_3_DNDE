using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.Infrastructure.Data;
using FinanceTrack.Finance.Infrastructure.Data.Queries;
using FinanceTrack.Finance.UseCases.Contributors.List;
using FinanceTrack.Finance.UseCases.FinancialTransactions.Expenses.List;
using FinanceTrack.Finance.UseCases.FinancialTransactions.Incomes.List;

namespace FinanceTrack.Finance.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        ConfigurationManager config,
        ILogger logger
    )
    {
        string connectionString = Guard.Against.NullOrEmpty(
            config.GetConnectionString("DefaultConnection")
        );
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services
            .AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
            .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>))
            .AddScoped<IListContributorsQueryService, ListContributorsQueryService>()
            .AddScoped<
                IListUserIncomeFinancialTransactionsQueryService,
                ListUserIncomeFinancialTransactionsQueryService
            >()
            .AddScoped<
                IListUserExpenseFinancialTransactionsQueryService,
                ListUserExpenseFinancialTransactionsQueryService
            >()
            .AddScoped<DeleteContributorService>()
            .AddScoped<CreateIncomeFinancialTransactionService>()
            .AddScoped<UpdateIncomeFinancialTransactionService>()
            .AddScoped<DeleteIncomeFinancialTransactionService>()
            .AddScoped<CreateExpenseFinancialTransactionService>()
            .AddScoped<UpdateExpenseFinancialTransactionService>()
            .AddScoped<DeleteExpenseFinancialTransactionService>();

        logger.LogInformation("{Project} services registered", "Infrastructure");

        return services;
    }
}
