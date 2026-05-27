using FinanceTrack.Finance.Core.TelegramBotLinkingSessionAggregate;
using FinanceTrack.Finance.Core.TelegramBotLinkingSessionAggregate.Specifications;
using FinanceTrack.Finance.Core.UserAggregate;
using FinanceTrack.Finance.Core.UserAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.Users.TelegramBotNotifications;

public sealed class GenerateLinkWithPrimaryCodeCommandHandler(
    IReadRepository<User> userRepo,
    IRepository<TelegramBotLinkingSession> sessionRepo,
    IUnitOfWork unitOfWork,
    ITelegramBotOptions botOptions
) : ICommandHandler<GenerateLinkWithPrimaryCodeCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        GenerateLinkWithPrimaryCodeCommand request,
        CancellationToken cancel
    )
    {
        var user = await userRepo.FirstOrDefaultAsync(
            new UserByIdSpec(Guid.Parse(request.UserId)),
            cancel
        );

        if (user is null)
            return Result.NotFound();

        if (user.IsTelegramNotificationsEnabled)
            return Result.Error("Telegram notifications are already connected.");

        var existing = await sessionRepo.FirstOrDefaultAsync(
            new TelegramBotLinkingSessionByUserIdSpec(request.UserId),
            cancel
        );

        if (existing is not null)
            await sessionRepo.DeleteAsync(existing, cancel);

        var primaryCode = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMinutes(botOptions.LinkingSessionLifetimeMinutes);
        var session = TelegramBotLinkingSession.Create(request.UserId, primaryCode, expiresAt);

        await sessionRepo.AddAsync(session, cancel);
        await unitOfWork.SaveChangesAsync(cancel);

        return Result.Success($"https://t.me/{botOptions.BotUsername}?start={primaryCode:N}");
    }
}
