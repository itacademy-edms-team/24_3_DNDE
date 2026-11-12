using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Elements;
using Sets.Domain.Sets;

namespace Sets.Application.Elements;

public sealed record CreateElementCommand(
    Guid SetId,
    string? Type,
    System.Text.Json.JsonElement? Payload,
    int? Position)
    : IRequest<ErrorOr<ElementResponse>>;

/// <summary>
/// Создаёт элемент и ставит его в указанную позицию.
/// Счётчик активных элементов, сдвиг позиций и вставка идут в одной транзакции 
/// под блокировкой строки набора на уровне БД: иначе параллельное изменение даст рассинхрон по позициям.
/// </summary>
public sealed class CreateElementHandler(
    ISetRepository sets,
    IElementRepository elements,
    ICurrentUser currentUser,
    TimeProvider clock)
    : IRequestHandler<CreateElementCommand, ErrorOr<ElementResponse>>
{
    public async Task<ErrorOr<ElementResponse>> Handle(
        CreateElementCommand command,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return SetErrors.UserUnresolved;

        var set = await sets.FindOwnedAsync(command.SetId, ownerId, cancellationToken);
        if (set is null)
            return SetErrors.NotFound;

        if (string.IsNullOrWhiteSpace(command.Type))
            return ElementErrors.TypeRequired;

        if (!ElementTypeNames.TryParse(command.Type, out var type))
            return ElementErrors.TypeUnsupported;

        if (command.Payload is not { } payloadJson)
            return ElementErrors.PayloadRequired;

        if (command.Position is < 0)
            return ElementErrors.PositionInvalid(command.Position.Value);

        // Схема payload определяется типом: новый формат - правка ElementPayloadJson
        var payload = ElementPayloadJson.Create(type, payloadJson);
        if (payload.IsError)
            return payload.Errors;

        var now = clock.GetUtcNow().UtcDateTime;

        return await elements.ExecuteLockedAsync<ErrorOr<ElementResponse>>(
            command.SetId,
            async (activeCount, token) =>
            {
                var position = command.Position ?? activeCount;
                if (position > activeCount)
                    return ElementErrors.PositionOutOfRange(position, activeCount);

                var created = Element.Create(command.SetId, payload.Value, position, now);
                if (created.IsError)
                    return created.Errors;

                await elements.ShiftPositionsFromAsync(command.SetId, position, token);
                elements.Add(created.Value);
                set.MarkContentChanged(now);
                await elements.SaveChangesAsync(token);

                return ElementResponse.From(created.Value);
            },
            cancellationToken);
    }
}
