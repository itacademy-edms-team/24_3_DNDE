using Common.Application.Idempotency;
using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Elements;
using Sets.Domain.Sets;

namespace Sets.Application.Elements;

/// <param name="IdempotencyKey">Значение заголовка Idempotency-Key; проверяется на HTTP-слое.</param>
public sealed record CreateElementCommand(
    Guid SetId,
    string? Type,
    System.Text.Json.JsonElement? Payload,
    int? Position,
    string IdempotencyKey)
    : IRequest<ErrorOr<ElementResponse>>
{
    /// <summary>
    /// Идентификатор операции в журнале идемпотентности.
    /// </summary>
    public const string Operation = "elements.create";
}

/// <summary>
/// Создаёт элемент и ставит его в указанную позицию (по умолчанию - в конец).
/// Внутри лока набора сначала проверяется ключ идемпотентности,
/// и только для новой операции считается позиция, выполняется сдвиг и вставка.
/// </summary>
public sealed class CreateElementHandler(
    ISetRepository sets,
    IElementRepository elements,
    IIdempotencyStore idempotency,
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

        var requestHash = CreateElementRequestHash.Compute(type, payload.Value, command.Position);
        var now = clock.GetUtcNow().UtcDateTime;

        return await elements.ExecuteLockedAsync<ErrorOr<ElementResponse>>(
            command.SetId,
            async (activeCount, token) => await idempotency.ExecuteAsync<ElementResponse>(
                ownerId,
                command.IdempotencyKey,
                CreateElementCommand.Operation,
                requestHash,
                operationBody: async innerToken =>
                {
                    var current = await elements.ListForOrderingAsync(command.SetId, innerToken);
                    var position = command.Position ?? current.Count;

                    var shifted = SetOrdering.ForInsert(current, position);
                    if (shifted.IsError)
                        return shifted.Errors;

                    var created = Element.Create(command.SetId, payload.Value, position, now);
                    if (created.IsError)
                        return created.Errors;

                    await elements.ApplyPositionsAsync(command.SetId, shifted.Value, innerToken);
                    elements.Add(created.Value);
                    set.MarkContentChanged(now);
                    await elements.SaveChangesAsync(innerToken);

                    return ElementResponse.From(created.Value);
                },
                token),
            cancellationToken);
    }
}
