using System.Reflection;
using Ardalis.SharedKernel;
using FinanceTrack.Finance.Core.ContributorAggregate;
using FinanceTrack.Finance.UseCases.Contributors.Create;

namespace FinanceTrack.Finance.Web.Configurations;

public static class MediatrConfigs
{
    public static IServiceCollection AddMediatrConfigs(this IServiceCollection services)
    {
        var mediatRAssemblies = new[]
        {
            Assembly.GetAssembly(typeof(Contributor)), // Core
            Assembly.GetAssembly(typeof(CreateContributorCommand)), // UseCases
        };

        services
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(mediatRAssemblies!))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

        return services;
    }
}
