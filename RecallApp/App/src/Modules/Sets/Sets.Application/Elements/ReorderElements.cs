using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Elements;
using Sets.Domain.Sets;

namespace Sets.Application.Elements;

public sealed record ReorderElementsCommand(Guid SetId, IReadOnlyList<Guid>? ElementIds)
    : IRequest<ErrorOr<Updated>>;

public sealed class ReorderElementsHandler(
    ISetRepository sets,
    IElementRepository elements,
    ICurrentUser currentUser,
    TimeProvider clock)
    : IRequestHandler<ReorderElementsCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(
        ReorderElementsCommand command,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return SetErrors.UserUnresolved;

        var set = await sets.FindOwnedAsync(command.SetId, ownerId, cancellationToken);
        if (set is null)
            return SetErrors.NotFound;

        if (command.ElementIds is not { Count: > 0 } requested)
            return ElementErrors.OrderRequired;

        var activeIds = await elements.ListActiveIdsAsync(command.SetId, cancellationToken);
        var requestedSet = requested.ToHashSet();
        var hasDuplicates = requestedSet.Count != requested.Count;
        var sameComposition = requestedSet.SetEquals(activeIds);

        // Элементы поменяли в другом окне
        if (hasDuplicates || !sameComposition)
            return ElementErrors.OrderMismatch;

        var now = clock.GetUtcNow().UtcDateTime;
        await elements.ApplyOrderAsync(command.SetId, requested, now, cancellationToken);
        set.MarkContentChanged(now);
        await elements.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}
