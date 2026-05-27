namespace FinanceTrack.Finance.UseCases.Users.TelegramBotNotifications;

public sealed record GetTelegramBotNotificationsStatusQuery(string UserId) : IQuery<Result<bool>>;
