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
/// Мягко удаляет элемент: соседние элементы сохраняют свои позиции, дырка в порядке допустима
/// (онтология §6.5), а сам набор живёт дальше.
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

                set.MarkElementsChanged(now);

                await elements.SaveChangesAsync(token);
                await sets.SaveChangesAsync(token);
                return Result.Deleted;
            },
            cancellationToken);
    }
}
