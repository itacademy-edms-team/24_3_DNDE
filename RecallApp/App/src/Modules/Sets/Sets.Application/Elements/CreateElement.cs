using Common.Application.Idempotency;
using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Media.Contracts;
using Sets.Application.Abstractions;
using Sets.Domain.Elements;
using Sets.Domain.Sets;

namespace Sets.Application.Elements;

/// <summary>
/// Создаёт элемент набора и ставит его в конец.
/// </summary>
/// <param name="SetId">Идентификатор набора.</param>
/// <param name="Type">Контрактное имя типа элемента.</param>
/// <param name="Payload">Содержимое элемента по схеме типа.</param>
/// <param name="IdempotencyKey">Значение заголовка Idempotency-Key; проверяется на HTTP-слое.</param>
/// <remarks>
/// <para>Элемент всегда встаёт в конец набора: ранг вычисляется после последнего элемента.</para>
/// <para>Проверка ключа идемпотентности идёт под локом набора,
/// поэтому параллельные запросы с одним ключом не создают два элемента.</para>
/// </remarks>
public sealed record CreateElementCommand(
    Guid SetId,
    string? Type,
    System.Text.Json.JsonElement? Payload,
    string IdempotencyKey)
    : IRequest<ErrorOr<ElementResponse>>
{
    /// <summary>
    /// Идентификатор операции в журнале идемпотентности.
    /// </summary>
    public const string Operation = "elements.create";
}

public sealed class CreateElementHandler(
    ISetRepository sets,
    IElementRepository elements,
    ISetsIdempotencyStore idempotency,
    IMediaReadiness mediaReadiness,
    IPositionGenerator positionGenerator,
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

        // Схема payload определяется типом: новый формат - правка ElementPayloadJson
        var payload = ElementPayloadJson.Create(type, payloadJson);
        if (payload.IsError)
            return payload.Errors;

        // Файлы проверяются до записи: иначе плохая ссылка попадёт в БД и всплывёт позже.
        var ready = await mediaReadiness.EnsureReadyAsync(
            ownerId, ElementMediaSlots.ImageIds(payload.Value), cancellationToken);
        if (ready.IsError)
            return ready.Errors;

        var requestHash = CreateElementRequestHash.Compute(type, payload.Value);
        var now = clock.GetUtcNow().UtcDateTime;

        return await elements.ExecuteLockedAsync<ErrorOr<ElementResponse>>(
            command.SetId,
            async (_, token) => await idempotency.ExecuteAsync<ElementResponse>(
                ownerId,
                command.IdempotencyKey,
                CreateElementCommand.Operation,
                requestHash,
                operationBody: async innerToken =>
                {
                    var last = await elements.GetLastAsync(command.SetId, innerToken);
                    var position = SetOrdering.PositionAtEnd(positionGenerator, last);

                    var created = Element.Create(command.SetId, payload.Value, position, now);
                    if (created.IsError)
                        return created.Errors;

                    elements.Add(created.Value);
                    set.MarkElementsChanged(now);

                    // Событие регистрируется на сущность, а в outbox его превратит interceptor (ADR 0040).
                    var scope = ElementMediaSlots.ElementScope(command.SetId, created.Value.Id);
                    created.Value.RegisterDomainEvent(new MediaReferencesChanged(
                        ownerId, scope, ElementMediaSlots.For(payload.Value, scope)));

                    await elements.SaveChangesAsync(innerToken);

                    return ElementResponse.From(created.Value);
                },
                token),
            cancellationToken);
    }
}
