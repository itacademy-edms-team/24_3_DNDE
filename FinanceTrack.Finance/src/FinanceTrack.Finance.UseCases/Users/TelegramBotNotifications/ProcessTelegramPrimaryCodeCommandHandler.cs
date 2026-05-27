using FinanceTrack.Finance.Core.TelegramBotLinkingSessionAggregate;
using FinanceTrack.Finance.Core.TelegramBotLinkingSessionAggregate.Specifications;
using FinanceTrack.Finance.Core.UserAggregate;
using FinanceTrack.Finance.Core.UserAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.Users.TelegramBotNotifications;

public sealed class ProcessTelegramPrimaryCodeCommandHandler(
    IReadRepository<User> userRepo,
    IRepository<TelegramBotLinkingSession> sessionRepo,
    IUnitOfWork unitOfWork
) : ICommandHandler<ProcessTelegramPrimaryCodeCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        ProcessTelegramPrimaryCodeCommand request,
        CancellationToken cancel
    )
    {
        var session = await sessionRepo.FirstOrDefaultAsync(
            new TelegramBotLinkingSessionByPrimaryCodeSpec(request.PrimaryCode),
            cancel
        );

        if (session is null)
            return Result.NotFound("Link code not found.");

        if (session.IsExpired)
            return Result.Error("Link code has expired.");

        var user = await userRepo.FirstOrDefaultAsync(
            new UserByIdSpec(Guid.Parse(session.UserId)),
            cancel
        );

        if (user is null)
            return Result.NotFound();

        if (user.IsTelegramNotificationsEnabled)
            return Result.Error("Telegram notifications are already connected.");

        var confirmationCode = Random.Shared.Next(100_000, 999_999);
        session.SetConfirmation(confirmationCode, request.TelegramChatId);

        await unitOfWork.SaveChangesAsync(cancel);

        return Result.Success(confirmationCode);
    }
}
