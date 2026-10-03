using FinanceTrack.Finance.Core.UserAggregate;
using FinanceTrack.Finance.Core.UserAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.Users.TelegramBotNotifications;

public sealed class DisableTelegramBotNotificationsCommandHandler(
    IRepository<User> repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<DisableTelegramBotNotificationsCommand, Result>
{
    public async Task<Result> Handle(
        DisableTelegramBotNotificationsCommand request,
        CancellationToken cancel
    )
    {
        var user = await repository.FirstOrDefaultAsync(
            new UserByIdSpec(Guid.Parse(request.UserId)),
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
