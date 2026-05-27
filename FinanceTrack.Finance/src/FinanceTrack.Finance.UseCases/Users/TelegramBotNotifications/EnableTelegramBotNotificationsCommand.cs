namespace FinanceTrack.Finance.UseCases.Users.TelegramBotNotifications;

public sealed record EnableTelegramBotNotificationsCommand(string UserId, int ConfirmationCode)
    : ICommand<Result>;
