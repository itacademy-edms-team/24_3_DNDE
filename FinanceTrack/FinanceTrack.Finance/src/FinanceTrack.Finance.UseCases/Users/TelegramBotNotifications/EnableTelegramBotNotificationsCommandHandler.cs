using FinanceTrack.Finance.Core.TelegramBotLinkingSessionAggregate;
using FinanceTrack.Finance.Core.TelegramBotLinkingSessionAggregate.Specifications;
using FinanceTrack.Finance.Core.UserAggregate;
using FinanceTrack.Finance.Core.UserAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.Users.TelegramBotNotifications;

public sealed class EnableTelegramBotNotificationsCommandHandler(
    IRepository<User> userRepo,
    IRepository<TelegramBotLinkingSession> sessionRepo,
    IUnitOfWork unitOfWork
) : ICommandHandler<EnableTelegramBotNotificationsCommand, Result>
{
    public async Task<Result> Handle(
        EnableTelegramBotNotificationsCommand request,
        CancellationToken cancel
    )
    {
        var session = await sessionRepo.FirstOrDefaultAsync(
            new TelegramBotLinkingSessionByConfirmationCodeSpec(
                request.UserId,
                request.ConfirmationCode
            ),
            cancel
        );

        if (session is null)
            return Result.NotFound("Confirmation code not found.");

        if (session.IsExpired)
            return Result.Error("Confirmation code has expired.");

        if (!session.TelegramChatId.HasValue)
            return Result.Error("Bot has not yet processed the link. Please try again.");

        var user = await userRepo.FirstOrDefaultAsync(
            new UserByIdSpec(Guid.Parse(request.UserId)),
            cancel
        );

        if (user is null)
            return Result.NotFound();

        user.ConnectTelegramBot(session.TelegramChatId.Value);

        await sessionRepo.DeleteAsync(session, cancel);
        await unitOfWork.SaveChangesAsync(cancel);

        return Result.Success();
    }
}
