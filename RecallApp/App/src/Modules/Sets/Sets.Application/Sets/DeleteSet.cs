using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Sets;

namespace Sets.Application.Sets;

/// <param name="Id">Id набора</param>
public sealed record DeleteSetCommand(Guid Id) : IRequest<ErrorOr<Deleted>>;

/// <summary>
/// Мягкое удаление набора и всех его элементов в одной операции:
/// набор скрывается, строки остаются до фоновой чистки.
/// </summary>
public sealed class DeleteSetHandler(
    ISetRepository sets,
    IElementRepository elements,
    ICurrentUser currentUser,
    TimeProvider clock)
    : IRequestHandler<DeleteSetCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteSetCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return SetErrors.UserUnresolved;

        var set = await sets.FindOwnedAsync(command.Id, ownerId, cancellationToken);
        if (set is null)
            return SetErrors.NotFound;

        var now = clock.GetUtcNow().UtcDateTime;
        set.MarkDeleted(now);
        await elements.MarkAllDeletedAsync(set.Id, now, cancellationToken);
        await sets.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
