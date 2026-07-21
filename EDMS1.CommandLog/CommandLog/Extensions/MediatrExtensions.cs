using EDMS1.CommandLog.Mediatr;
using Microsoft.Extensions.DependencyInjection;

namespace EDMS1.CommandLog.Extensions;

public static class MediatrExtensions
{
    public static MediatRServiceConfiguration AddCommandLogBehavior(this MediatRServiceConfiguration configuration)
    {
        return configuration.AddOpenBehavior(typeof(CommandLogBehavior<,>));
    }
}