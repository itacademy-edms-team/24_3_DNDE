using System.Text.Json;
using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Elements;
using Sets.Domain.Sets;

namespace Sets.Application.Elements;

public sealed record UpdateElementCommand(Guid SetId, Guid ElementId, JsonElement? Payload)
    : IRequest<ErrorOr<ElementResponse>>;

/// <summary>
/// Полная замена содержимого элемента.
/// Тип элемента менять нельзя.
/// </summary>
public sealed class UpdateElementHandler(
    ISetRepository sets,
    IElementRepository elements,
    ICurrentUser currentUser,
    TimeProvider clock)
    : IRequestHandler<UpdateElementCommand, ErrorOr<ElementResponse>>
{
    public async Task<ErrorOr<ElementResponse>> Handle(
        UpdateElementCommand command,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return SetErrors.UserUnresolved;

        var set = await sets.FindOwnedAsync(command.SetId, ownerId, cancellationToken);
        if (set is null)
            return SetErrors.NotFound;

        var element = await elements.FindAsync(command.SetId, command.ElementId, cancellationToken);
        if (element is null)
            return ElementErrors.NotFound;

        if (command.Payload is not { } payloadJson)
            return ElementErrors.PayloadRequired;

        var payload = ElementPayloadJson.Create(element.Type, payloadJson);
        if (payload.IsError)
            return payload.Errors;

        var now = clock.GetUtcNow().UtcDateTime;
        var replaced = element.ReplaceContent(payload.Value, now);
        if (replaced.IsError)
            return replaced.Errors;

        set.MarkContentChanged(now);
        await elements.SaveChangesAsync(cancellationToken);

        return ElementResponse.From(element);
    }
}
