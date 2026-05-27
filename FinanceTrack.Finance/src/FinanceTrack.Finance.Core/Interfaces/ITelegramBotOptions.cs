namespace FinanceTrack.Finance.Core.Interfaces;

public interface ITelegramBotOptions
{
    string BotUsername { get; }
    int LinkingSessionLifetimeMinutes { get; }
}
