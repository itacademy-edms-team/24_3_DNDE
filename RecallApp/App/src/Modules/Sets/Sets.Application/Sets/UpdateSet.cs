using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Sets;

namespace Sets.Application.Sets;

/// <param name="Id">Id набора.</param>
/// <param name="Title">Новое название: PUT присылает полное состояние.</param>
/// <param name="Description">Новое описание; null или пустая строка, чтобы затереть.</param>
public sealed record UpdateSetCommand(Guid Id, string? Title, string? Description) : IRequest<ErrorOr<SetResponse>>;

public sealed class UpdateSetHandler(ISetRepository repository, ICurrentUser currentUser, TimeProvider clock)
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
        return SetResponse.From(set);
    }
}
