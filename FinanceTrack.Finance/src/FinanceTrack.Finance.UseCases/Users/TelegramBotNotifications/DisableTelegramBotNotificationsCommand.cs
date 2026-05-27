namespace FinanceTrack.Finance.UseCases.Users.TelegramBotNotifications;

public sealed record DisableTelegramBotNotificationsCommand(string UserId) : ICommand<Result>;
