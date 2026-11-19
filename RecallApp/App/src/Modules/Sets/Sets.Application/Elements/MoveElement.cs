using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Elements;
using Sets.Domain.Sets;

namespace Sets.Application.Elements;

/// <summary>
/// Перемещает элемент между указанными соседями.
/// </summary>
/// <param name="SetId">Набор, в котором перемещается элемент.</param>
/// <param name="ElementId">Перемещаемый элемент.</param>
/// <param name="PreviousId">Сосед перед элементом; <see langword="null"/> - начало набора.</param>
/// <param name="NextId">Сосед после элемента; <see langword="null"/> - конец набора.</param>
public sealed record MoveElementCommand(Guid SetId, Guid ElementId, Guid? PreviousId, Guid? NextId)
    : IRequest<ErrorOr<ElementResponse>>;

/// <summary>
/// Переставляет элемент по соседям: новый ранг лежит между ними, остальные элементы не меняются.
/// </summary>
/// <remarks>
/// Операция под локом набора: соседи проверяются и позиция вычисляется на одном состоянии.
/// Повтор с теми же соседями даёт тот же ранг, а изменившиеся соседи возвращают ошибку,
/// поэтому отдельный ключ идемпотентности не нужен.
/// </remarks>
public sealed class MoveElementHandler(
    ISetRepository sets,
    IElementRepository elements,
    IPositionGenerator positionGenerator,
    ICurrentUser currentUser,
    TimeProvider clock)
    : IRequestHandler<MoveElementCommand, ErrorOr<ElementResponse>>
{
    public async Task<ErrorOr<ElementResponse>> Handle(
        MoveElementCommand command,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return SetErrors.UserUnresolved;

        var set = await sets.FindOwnedAsync(command.SetId, ownerId, cancellationToken);
        if (set is null)
            return SetErrors.NotFound;

        var now = clock.GetUtcNow().UtcDateTime;

        return await elements.ExecuteLockedAsync<ErrorOr<ElementResponse>>(
            command.SetId,
            async (_, token) =>
            {
                // Мутируемый элемент должен быть в трекинге: список для соседей приходит без отслеживания.
                var element = await elements.FindAsync(command.SetId, command.ElementId, token);
                if (element is null)
                    return ElementErrors.NotFound;

                var current = await elements.ListForOrderingAsync(command.SetId, token);

                var position = SetOrdering.PositionFor(
                    positionGenerator, current, command.ElementId, command.PreviousId, command.NextId);
                if (position.IsError)
                    return position.Errors;
                
                if (string.Equals(element.Position, position.Value.Value, StringComparison.Ordinal))
                    return ElementResponse.From(element);

                element.MoveTo(position.Value);
                set.MarkElementsChanged(now);

                await elements.SaveChangesAsync(token);
                await sets.SaveChangesAsync(token);

                return ElementResponse.From(element);
            },
            cancellationToken);
    }
}
