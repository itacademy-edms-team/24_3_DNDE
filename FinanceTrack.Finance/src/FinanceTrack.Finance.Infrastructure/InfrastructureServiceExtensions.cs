using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.Infrastructure.Data;
using FinanceTrack.Finance.Infrastructure.Data.Queries;
using FinanceTrack.Finance.Infrastructure.PdfImport;
using FinanceTrack.Finance.UseCases.Analytics;
using FinanceTrack.Finance.UseCases.FullTextSearch;
using FinanceTrack.Finance.UseCases.ImportTransactions;
using FinanceTrack.Finance.UseCases.Wallets;

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
            .AddScoped<CreateWalletService>()
            .AddScoped<CreateIncomeService>()
            .AddScoped<CreateExpenseService>()
            .AddScoped<TransferService>()
            .AddScoped<UpdateTransactionService>()
            .AddScoped<DeleteTransactionService>()
            .AddScoped<RecurringTransactionProcessorService>()
            // Import services
            .AddScoped<IBankStatementImportService, BankStatementImportService>()
            .AddScoped<ImportTransactionsService>()
            // Query services
            .AddScoped<IGeneralAnalyticsQueryService, GeneralAnalyticsQueryService>()
            .AddScoped<IWalletAnalyticsQueryService, WalletAnalyticsQueryService>()
            .AddScoped<IWalletMetadataQueryService, WalletMetadataQueryService>()
            .AddScoped<IWalletForecastQueryService, WalletForecastQueryService>()
            .AddScoped<
                IGlobalFullTextSearchQueryService,
                GlobalFullTextSearchParallelQueryService
            >();

        logger.LogInformation("{Project} services registered", "Infrastructure");

        return services;
    }
}
