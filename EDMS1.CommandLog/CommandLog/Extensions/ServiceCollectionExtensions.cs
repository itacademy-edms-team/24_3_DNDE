using EDMS1.CommandLog.Commands;
using EDMS1.CommandLog.Models;
using EDMS1.CommandLog.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EDMS1.CommandLog.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрация логирующихся команд.
    /// </summary>
    public static IServiceCollection AddCommandLogService(this IServiceCollection services, Type assemblyMarkerType)
    {
        var dictionary = assemblyMarkerType.Assembly
            .GetTypes()
            .Where(x => x.IsAssignableTo(typeof(ICommand)) && x.IsClass && !x.IsAbstract)
            .ToDictionary(x => x.Name);

        services.AddSingleton<ICommandTypes>(_ => new CommandTypes(dictionary));

        services.AddScoped<ICommandLogService, CommandLogService>();

        return services;
    }
}