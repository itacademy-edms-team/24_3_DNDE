namespace FinanceTrack.Finance.UseCases.Users.TelegramBotNotifications;

public sealed record ProcessTelegramPrimaryCodeCommand(Guid PrimaryCode, long TelegramChatId)
    : ICommand<Result<int>>;
