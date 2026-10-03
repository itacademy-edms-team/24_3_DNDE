using FinanceTrack.Finance.Core.UserAggregate;
using FinanceTrack.Finance.Core.UserAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.Users.TelegramBotNotifications;

public sealed class GetTelegramBotNotificationsStatusQueryHandler(IReadRepository<User> repository)
    : IQueryHandler<GetTelegramBotNotificationsStatusQuery, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        GetTelegramBotNotificationsStatusQuery request,
        CancellationToken cancel
    )
    {
        var user = await repository.FirstOrDefaultAsync(
            new UserByIdSpec(Guid.Parse(request.UserId)),
            cancel
        );

        if (user is null)
            return Result.NotFound();

        return Result.Success(user.IsTelegramNotificationsEnabled);
    }
}
