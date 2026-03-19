using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Ardalis.SharedKernel;
using FinanceTrack.Finance.Infrastructure.Data;
using FinanceTrack.Finance.Infrastructure.Data.Queries;
using FinanceTrack.Finance.Web.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinanceTrack.Finance.Benchmarks.Benchmarks;

public static class GlobalSearchBenchContext
{
    public static IServiceProvider ServiceProvider { get; }

    static GlobalSearchBenchContext()
    {
        var configuration = new ConfigurationManager();

        configuration
            .AddUserSecrets(Assembly.GetAssembly(typeof(ServiceConfigs))!)
            .AddEnvironmentVariables();
        var connectionString = Guard.Against.NullOrEmpty(
            configuration.GetConnectionString("DefaultConnection")
        );

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger("Benchmarks");

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddDbContextFactory<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IDomainEventDispatcher, NoOpDomainEventDispatcher>();
        services.AddScoped<GlobalFullTextSearchQueryService>();
        services.AddScoped<GlobalFullTextSearchParallelQueryService>();

        ServiceProvider = services.BuildServiceProvider();

        logger.LogInformation("Benchmark DI configured");
    }
}
