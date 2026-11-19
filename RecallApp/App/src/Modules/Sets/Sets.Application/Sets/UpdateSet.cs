using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Sets;

namespace Sets.Application.Sets;

/// <summary>
/// Заменяет название и описание набора целиком.
/// </summary>
/// <param name="Id">Идентификатор набора.</param>
/// <param name="Title">Новое название; обязательно.</param>
/// <param name="Description">Новое описание; <see langword="null"/> и пробельная строка
/// означают отсутствие описания.</param>
/// <remarks>
/// Запрос присылает полное состояние набора: непереданное поле не сохраняется, а заменяется.
/// Если значения не изменились с последнего изменения,
/// набор не перезаписывается и <c>updated_at</c> не двигается.
/// </remarks>
public sealed record UpdateSetCommand(Guid Id, string? Title, string? Description) : IRequest<ErrorOr<SetResponse>>;

public sealed class UpdateSetHandler(
    ISetRepository repository,
    IElementRepository elements,
    ICurrentUser currentUser,
    TimeProvider clock)
    : IRequestHandler<UpdateSetCommand, ErrorOr<SetResponse>>
{
    public async Task<ErrorOr<SetResponse>> Handle(UpdateSetCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return SetErrors.UserUnresolved;

        var set = await repository.FindOwnedAsync(command.Id, ownerId, cancellationToken);
        if (set is null)
            return SetErrors.NotFound;

        var replaced = set.Replace(command.Title, command.Description, clock.GetUtcNow().UtcDateTime);
        if (replaced.IsError)
            return replaced.Errors;

        await repository.SaveChangesAsync(cancellationToken);

        var elementCount = await elements.CountActiveAsync(set.Id, cancellationToken);
        return SetResponse.From(set, elementCount);
    }
}
