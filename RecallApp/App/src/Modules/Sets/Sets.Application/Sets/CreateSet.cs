using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Sets;

namespace Sets.Application.Sets;

/// <param name="Title">Название; отсутствие или пустая строка - ошибка Set.TitleRequired.</param>
/// <param name="Description">Описание необязательно.</param>
public sealed record CreateSetCommand(string? Title, string? Description) : IRequest<ErrorOr<SetResponse>>;

public sealed class CreateSetHandler(ISetRepository repository, ICurrentUser currentUser, TimeProvider clock)
    : IRequestHandler<CreateSetCommand, ErrorOr<SetResponse>>
{
    public async Task<ErrorOr<SetResponse>> Handle(CreateSetCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return SetErrors.UserUnresolved;

        var created = Set.Create(ownerId, command.Title, command.Description, clock.GetUtcNow().UtcDateTime);
        if (created.IsError)
            return created.Errors;

        repository.Add(created.Value);
        await repository.SaveChangesAsync(cancellationToken);
        return SetResponse.From(created.Value);
    }
}
