using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Elements;
using Sets.Domain.Sets;

namespace Sets.Application.Elements;

/// <param name="SetId">Набор из маршрута.</param>
/// <param name="ElementIds">Все активные id набора в новом порядке; частичная перестановка не поддерживается.</param>
public sealed record ReorderElementsCommand(Guid SetId, IReadOnlyList<Guid>? ElementIds)
    : IRequest<ErrorOr<Updated>>;

/// <summary>
/// Полный пересчёт порядка: клиент присылает все id, сервер присваивает позиции `0..n-1`.
/// Проверка состава и вычисление целевых позиций - в домене (<see cref="SetOrdering.Assign"/>);
/// сверка и запись идут под одним локом набора.
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

        var now = clock.GetUtcNow().UtcDateTime;

        return await elements.ExecuteLockedAsync<ErrorOr<Updated>>(
            command.SetId,
            async (_, token) =>
            {
                var current = await elements.ListForOrderingAsync(command.SetId, token);

                var assignments = SetOrdering.Assign(current, command.ElementIds);
                if (assignments.IsError)
                    return assignments.Errors;

                var currentPositions = current.ToDictionary(element => element.Id, element => element.Position);
                var moved = assignments.Value.Count(assignment =>
                    currentPositions[assignment.ElementId] != assignment.Position);

                // Порядок уже такой - не считаем это изменением набора.
                if (moved == 0)
                    return Result.Updated;

                await elements.ApplyPositionsAsync(command.SetId, assignments.Value, token);
                set.MarkElementsChanged(now);
                await sets.SaveChangesAsync(token);
                return Result.Updated;
            },
            cancellationToken);
    }
}
