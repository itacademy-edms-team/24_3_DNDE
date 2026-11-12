using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Elements;
using Sets.Domain.Sets;

namespace Sets.Application.Elements;

public sealed record DeleteElementCommand(Guid SetId, Guid ElementId) : IRequest<ErrorOr<Deleted>>;

/// <summary>
/// Мягкое удаление элемента набора и пересчёт позиций в этом же процессе.
/// </summary>
public sealed class DeleteElementHandler(
    ISetRepository sets,
    IElementRepository repo,
    ICurrentUser currentUser,
    TimeProvider clock)
    : IRequestHandler<DeleteElementCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteElementCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return SetErrors.UserUnresolved;

        var set = await sets.FindOwnedAsync(command.SetId, ownerId, cancellationToken);
        if (set is null)
            return SetErrors.NotFound;

        var now = clock.GetUtcNow().UtcDateTime;
        var deleted = await repo.DeleteAndCompactAsync(command.SetId, command.ElementId, now, cancellationToken);
        if (!deleted)
            return ElementErrors.NotFound;

        set.MarkContentChanged(now);
        await sets.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
