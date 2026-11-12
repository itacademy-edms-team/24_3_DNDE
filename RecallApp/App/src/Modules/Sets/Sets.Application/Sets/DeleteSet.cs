using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Sets;

namespace Sets.Application.Sets;

public sealed record DeleteSetCommand(Guid Id) : IRequest<ErrorOr<Deleted>>;

/// <summary>
/// Мягкое удаление набора и всех его элементов в одной операции: набор скрывается,
/// строки остаются до фоновой чистки (онтология §6.6, ступень 1; ADR 0036).
/// Пометить набор и его элементы нужно под локом набора: иначе параллельная вставка
/// успеет добавить элемент в уже удаляемый набор.
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

        return await elements.ExecuteLockedAsync<ErrorOr<Deleted>>(
            set.Id,
            async (_, token) =>
            {
                set.MarkDeleted(now);
                await elements.MarkAllDeletedAsync(set.Id, now, token);
                await sets.SaveChangesAsync(token);
                return Result.Deleted;
            },
            cancellationToken);
    }
}
