namespace FinanceTrack.Finance.UseCases.Users.TelegramBotNotifications;

public sealed record GenerateLinkWithPrimaryCodeCommand(string UserId) : ICommand<Result<string>>;
