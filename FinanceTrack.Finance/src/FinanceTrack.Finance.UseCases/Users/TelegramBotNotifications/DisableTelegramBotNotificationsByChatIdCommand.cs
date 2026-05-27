namespace FinanceTrack.Finance.UseCases.Users.TelegramBotNotifications;

public sealed record DisableTelegramBotNotificationsByChatIdCommand(long TelegramChatId)
    : ICommand<Result>;
