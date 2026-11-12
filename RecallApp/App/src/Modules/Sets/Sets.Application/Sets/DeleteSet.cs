using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Sets;

namespace Sets.Application.Sets;

/// <param name="Id">Id набора</param>
public sealed record DeleteSetCommand(Guid Id) : IRequest<ErrorOr<Deleted>>;

/// <summary>Мягкое удаление: набор скрывается, строка и элементы остаются до фоновой чистки.</summary>
public sealed class DeleteSetHandler(ISetRepository repository, ICurrentUser currentUser, TimeProvider clock)
    : IRequestHandler<DeleteSetCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteSetCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return SetErrors.UserUnresolved;

        var set = await repository.FindOwnedAsync(command.Id, ownerId, cancellationToken);
        if (set is null)
            return SetErrors.NotFound;

        set.MarkDeleted(clock.GetUtcNow().UtcDateTime);
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Deleted;
    }
}
