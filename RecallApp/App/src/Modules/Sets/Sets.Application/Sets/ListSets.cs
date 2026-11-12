using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Sets;

namespace Sets.Application.Sets;

/// <summary>Список наборов текущего пользователя.</summary>
public sealed record ListSetsQuery : IRequest<ErrorOr<IReadOnlyList<SetSummaryResponse>>>;

public sealed class ListSetsHandler(ISetRepository repository, ICurrentUser currentUser)
    : IRequestHandler<ListSetsQuery, ErrorOr<IReadOnlyList<SetSummaryResponse>>>
{
    public async Task<ErrorOr<IReadOnlyList<SetSummaryResponse>>> Handle(
        ListSetsQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return SetErrors.UserUnresolved;

        var sets = await repository.ListOwnedAsync(ownerId, cancellationToken);
        return sets.Select(SetSummaryResponse.From).ToList();
    }
}
