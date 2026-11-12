using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Elements;
using Sets.Domain.Sets;

namespace Sets.Application.Elements;

public sealed record DeleteElementCommand(Guid SetId, Guid ElementId) : IRequest<ErrorOr<Deleted>>;

/// <summary>
/// Мягкое удаление элемента с уплотнением позиций: набор остаётся плотным `0..n-1`
/// (онтология §6.5), а сам набор живёт дальше. Проверка существования, удаление и сдвиг
/// идут под блокировкой набора - тот же лок, что у вставки и перестановки.
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
                var deleted = await elements.DeleteAndCompactAsync(command.SetId, command.ElementId, now, token);
                if (!deleted)
                    return ElementErrors.NotFound;

                set.MarkContentChanged(now);
                await sets.SaveChangesAsync(token);
                return Result.Deleted;
            },
            cancellationToken);
    }
}
