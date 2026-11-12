using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Elements;
using Sets.Domain.Sets;

namespace Sets.Application.Elements;

/// <param name="ElementIds">Все активные id набора в новом порядке; частичная перестановка не поддерживается.</param>
public sealed record ReorderElementsCommand(Guid SetId, IReadOnlyList<Guid>? ElementIds)
    : IRequest<ErrorOr<Updated>>;

/// <summary>
/// Полный пересчёт порядка элементов: клиент присылает все id, сервер присваивает позиции `0..n-1`.
/// Сверка состава и запись идут под одним локом набора: иначе между проверкой и применением могла бы вклиниться вставка,
/// и новый элемент остался бы за пределами диапазона позиций.
/// Несовпадение набора id означает, что состав изменился между чтением и записью - 409.
/// </summary>
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

        var requestedSet = requested.ToHashSet();
        if (requestedSet.Count != requested.Count)
            return ElementErrors.OrderMismatch;

        var now = clock.GetUtcNow().UtcDateTime;

        return await elements.ExecuteLockedAsync<ErrorOr<Updated>>(
            command.SetId,
            async (_, token) =>
            {
                var activeIds = await elements.ListActiveIdsAsync(command.SetId, token);
                if (!requestedSet.SetEquals(activeIds))
                    return ElementErrors.OrderMismatch;

                await elements.ApplyOrderAsync(command.SetId, requested, now, token);
                set.MarkContentChanged(now);
                await sets.SaveChangesAsync(token);
                return Result.Updated;
            },
            cancellationToken);
    }
}
