using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.Infrastructure.Configurations;

public class TelegramBotOptions : ITelegramBotOptions
{
    public const string SectionName = "Notifications:TelegramBot";

    public string BotToken { get; set; } = string.Empty;
    public string BotUsername { get; set; } = string.Empty;
    public int LinkingSessionLifetimeMinutes { get; set; } = 15;

    public bool IsConfigured => !string.IsNullOrEmpty(BotToken);
}
