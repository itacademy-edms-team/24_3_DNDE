using FinanceTrack.Finance.Infrastructure.Configurations;
using Microsoft.Extensions.Hosting;
using TelegramBotBase.Builder;

namespace FinanceTrack.Finance.Infrastructure.Notifications.Telegram;

public sealed class TelegramBotHostedService(
    IOptions<TelegramBotOptions> options,
    IServiceProvider serviceProvider,
    ILogger<TelegramBotHostedService> logger
) : IHostedService
{
    private TelegramBotBase.BotBase? _bot;

    public async Task StartAsync(CancellationToken cancel)
    {
        var config = options.Value;

        if (!config.IsConfigured)
        {
            logger.LogInformation("Telegram bot is not configured - skipping startup.");
            return;
        }

        _bot = BotBaseBuilder
            .Create()
            .WithAPIKey(config.BotToken)
            .DefaultMessageLoop()
            .WithServiceProvider<StartForm>(serviceProvider)
            .NoProxy()
            .CustomCommands(_ => { })
            .NoSerialization()
            .UseRussian()
            .UseThreadPool(2, 2)
            .Build();

        // Логируем исключения бота
        _bot.Exception += (_, e) =>
            logger.LogError(
                e.Error,
                "Telegram bot exception for DeviceId={DeviceId} Command={Command}",
                e.DeviceId,
                e.Command
            );

        await _bot.Start();

        logger.LogInformation("Telegram bot started: @{BotUsername}", config.BotUsername);
    }

    public async Task StopAsync(CancellationToken cancel)
    {
        if (_bot is null)
            return;

        await _bot.Stop();
        logger.LogInformation("Telegram bot stopped.");
    }
}
