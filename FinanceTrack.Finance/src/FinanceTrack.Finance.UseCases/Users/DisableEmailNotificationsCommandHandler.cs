using FinanceTrack.Finance.Core.UserAggregate;
using FinanceTrack.Finance.Core.UserAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.Users;

public sealed class DisableEmailNotificationsCommandHandler(
    IRepository<User> repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<DisableEmailNotificationsCommand, Result>
{
    public async Task<Result> Handle(
        DisableEmailNotificationsCommand request,
        CancellationToken cancellationToken
    )
    {
        var user = await repository.FirstOrDefaultAsync(
            new UserByIdSpec(Guid.Parse(request.UserId)),
            cancellationToken
        );

        if (user is null)
            return Result.NotFound();

        user.DisableEmailNotifications();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
