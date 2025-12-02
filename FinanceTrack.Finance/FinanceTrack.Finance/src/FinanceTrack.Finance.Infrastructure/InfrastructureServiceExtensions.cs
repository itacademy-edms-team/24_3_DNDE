using FinanceTrack.Finance.Core.Interfaces;
using FinanceTrack.Finance.Core.Services;
using FinanceTrack.Finance.Core.TransactionAggregate;
using FinanceTrack.Finance.Infrastructure.Data;
using FinanceTrack.Finance.Infrastructure.Data.Queries;
using FinanceTrack.Finance.UseCases.Contributors.List;

namespace FinanceTrack.Finance.Infrastructure;

public static class InfrastructureServiceExtensions
{
  public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    ConfigurationManager config,
    ILogger logger
  )
  {
    string? connectionString = config.GetConnectionString("DefaultConnection");
    Guard.Against.Null(connectionString);
    services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

    services
      .AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
      .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>))
      .AddScoped<IListContributorsQueryService, ListContributorsQueryService>()
      .AddScoped<IDeleteContributorService, DeleteContributorService>();

    logger.LogInformation("{Project} services registered", "Infrastructure");

    return services;
  }
}
