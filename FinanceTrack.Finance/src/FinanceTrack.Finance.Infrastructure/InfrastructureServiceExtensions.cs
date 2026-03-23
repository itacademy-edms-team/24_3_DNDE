using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.Infrastructure.Data;
using FinanceTrack.Finance.Infrastructure.Data.Queries;
using FinanceTrack.Finance.UseCases.Analytics;
using FinanceTrack.Finance.UseCases.FullTextSearch;

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
            .AddScoped<IUnitOfWork, EfUnitOfWork>()
            // Domain services
            .AddScoped<CreateIncomeService>()
            .AddScoped<CreateExpenseService>()
            .AddScoped<TransferService>()
            .AddScoped<UpdateTransactionService>()
            .AddScoped<DeleteTransactionService>()
            .AddScoped<RecurringTransactionProcessorService>()
            // Query services
            .AddScoped<IGeneralAnalyticsQueryService, GeneralAnalyticsQueryService>()
            .AddScoped<IWalletAnalyticsQueryService, WalletAnalyticsQueryService>()
            .AddScoped<
                IGlobalFullTextSearchQueryService,
                GlobalFullTextSearchParallelQueryService
            >();

        logger.LogInformation("{Project} services registered", "Infrastructure");

        return services;
    }
}
