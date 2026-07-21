using System.Diagnostics;
using EDMS1.CommandLog.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EDMS1.CommandLog.BackgroundServices;

/// <summary>
/// Сервис повторной обработки команд.
/// </summary>
public class CommandLogRetryBackgroundService : BackgroundService
{
    private const double Period = 12;

    private readonly IServiceProvider _serviceProvider;
    private readonly PeriodicTimer _timer = new(TimeSpan.FromMinutes(Period));
    private readonly ILogger<CommandLogRetryBackgroundService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLogRetryBackgroundService"/> class.
    /// </summary>
    public CommandLogRetryBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<CommandLogRetryBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            using var activity = new Activity(nameof(CommandLogRetryBackgroundService));

            activity.Start();

            try
            {
                using var scope = _serviceProvider.CreateScope();
                scope.SetScopedProviderInAccessor(true);

                var commandLogService =
                    scope.ServiceProvider.GetRequiredService<ICommandLogService>();

                await commandLogService.RetryCommandsAsync(stoppingToken);

                scope.ClearScopedProviderInAccessor();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
            }
        }
        while (await _timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested);
    }
}
