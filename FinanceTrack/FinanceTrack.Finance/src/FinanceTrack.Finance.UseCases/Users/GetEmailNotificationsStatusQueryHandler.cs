using FinanceTrack.Finance.Core.UserAggregate;
using FinanceTrack.Finance.Core.UserAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.Users;

public class GetEmailNotificationsStatusQueryHandler(IReadRepository<User> repository)
    : IQueryHandler<GetEmailNotificationsStatusQuery, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        GetEmailNotificationsStatusQuery request,
        CancellationToken cancel
    )
    {
        var user = await repository.FirstOrDefaultAsync(
            new UserByIdSpec(Guid.Parse(request.UserId)),
            cancel
        );

        if (user is null)
            return Result.NotFound();

        return Result.Success(user.IsEmailNotificationsEnabled);
    }
}
