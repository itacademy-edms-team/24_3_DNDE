using EDMS1.CommandLog.BackgroundServices;
using EDMS1.CommandLog.Commands;
using EDMS1.CommandLog.Helpers;
using EDMS1.CommandLog.Models;
using EDMS1.CommandLog.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EDMS1.CommandLog.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// <para>
    /// Регистрация логирующихся команд.
    /// </para>
    /// <remarks>
    /// Пример регистрации CommandLogService взят из <see href="https://github.com/dotnet/eShop/blob/main/src/Catalog.API/Extensions/Extensions.cs">eShop</see>
    /// </remarks>
    /// </summary>
    public static IServiceCollection AddCommandLogService<TContext>(this IServiceCollection services, Type assemblyMarkerType)
        where TContext : DbContext
    {
        var dictionary = assemblyMarkerType.Assembly
            .GetTypes()
            .Where(x => x.IsAssignableTo(typeof(ICommand)) && x.IsClass && !x.IsAbstract)
            .ToDictionary(x => x.Name);

        services.AddSingleton<ICommandTypes>(_ => new CommandTypes(dictionary));
        
        services.AddScoped<ICommandLogService, CommandLogService<TContext>>();
        
        services.AddScoped<ICommandContext, CommandContext>();
        
        services.AddHttpContextAccessor();
        services.AddSingleton<ServiceProviderAccessor>();
        services.AddHostedService<CommandLogRetryBackgroundService>();

        return services;
    }
}