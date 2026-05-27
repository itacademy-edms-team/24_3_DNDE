using FinanceTrack.Finance.Core.UserAggregate;
using FinanceTrack.Finance.Core.UserAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.Users.TelegramBotNotifications;

public sealed class DisableTelegramBotNotificationsByChatIdCommandHandler(
    IRepository<User> repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<DisableTelegramBotNotificationsByChatIdCommand, Result>
{
    public async Task<Result> Handle(
        DisableTelegramBotNotificationsByChatIdCommand request,
        CancellationToken cancel
    )
    {
        var user = await repository.FirstOrDefaultAsync(
            new UserByTelegramChatIdSpec(request.TelegramChatId),
            cancel
        );

        if (user is null)
            return Result.NotFound();

        if (!user.IsTelegramNotificationsEnabled)
            return Result.Error("Telegram notifications are not connected.");

        user.DisconnectTelegramBot();

        await unitOfWork.SaveChangesAsync(cancel);

        return Result.Success();
    }
}
