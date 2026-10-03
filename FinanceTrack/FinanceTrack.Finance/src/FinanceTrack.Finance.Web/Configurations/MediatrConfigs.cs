using System.Reflection;
using Ardalis.SharedKernel;
using FinanceTrack.Finance.Core.WalletAggregate;
using FinanceTrack.Finance.UseCases.Wallets.Create;

namespace FinanceTrack.Finance.Web.Configurations;

public static class MediatrConfigs
{
    public static IServiceCollection AddMediatrConfigs(this IServiceCollection services)
    {
        var mediatRAssemblies = new[]
        {
            Assembly.GetAssembly(typeof(Wallet)), // Core
            Assembly.GetAssembly(typeof(CreateWalletCommand)), // UseCases
        };

        services
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(mediatRAssemblies!))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

        return services;
    }
}
