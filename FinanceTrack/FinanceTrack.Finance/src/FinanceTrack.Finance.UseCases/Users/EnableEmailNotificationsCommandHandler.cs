using FinanceTrack.Finance.Core.UserAggregate;
using FinanceTrack.Finance.Core.UserAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.Users;

public sealed class EnableEmailNotificationsCommandHandler(
    IRepository<User> repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<EnableEmailNotificationsCommand, Result>
{
    public async Task<Result> Handle(
        EnableEmailNotificationsCommand request,
        CancellationToken cancel
    )
    {
        var user = await repository.FirstOrDefaultAsync(
            new UserByIdSpec(Guid.Parse(request.UserId)),
            cancel
        );

        if (user is null)
            return Result.NotFound();

        user.EnableEmailNotifications();

        await unitOfWork.SaveChangesAsync(cancel);

        return Result.Success();
    }
}
