using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Media.Contracts;
using Sets.Application.Abstractions;
using Sets.Domain.Elements;
using Sets.Domain.Sets;

namespace Sets.Application.Elements;

public sealed record DeleteElementCommand(Guid SetId, Guid ElementId) : IRequest<ErrorOr<Deleted>>;

/// <summary>
/// Мягкое удаление элемента с уплотнением позиций: набор остаётся плотным `0..n-1`
/// (онтология §6.5), а сам набор живёт дальше.
/// За решение по сдвигу отвечает (<see cref="SetOrdering.ForRemoval"/>).
/// </summary>
public sealed class DeleteElementHandler(
    ISetRepository sets,
    IElementRepository elements,
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

        return await elements.ExecuteLockedAsync<ErrorOr<Deleted>>(
            command.SetId,
            async (_, token) =>
            {
                var element = await elements.FindAsync(command.SetId, command.ElementId, token);
                if (element is null)
                    return ElementErrors.NotFound;

                element.MarkDeleted(now);

                // Элемент исчезает - его слоты надо снять: пустой набор означает «отвязать всё».
                element.RegisterDomainEvent(new MediaReferencesChanged(
                    ownerId, ElementMediaSlots.ElementScope(command.SetId, element.Id), []));

                // Удаление фиксируется до сдвига: частичный уникальный индекс 
                // не пустит соседа на ещё занятую позицию.
                await elements.SaveChangesAsync(token);

                var remaining = await elements.ListForOrderingAsync(command.SetId, token);
                await elements.ApplyPositionsAsync(
                    command.SetId,
                    SetOrdering.ForRemoval(remaining, element.Position),
                    token);

                set.MarkElementsChanged(now);
                await sets.SaveChangesAsync(token);
                return Result.Deleted;
            },
            cancellationToken);
    }
}
