using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Sets;

namespace Sets.Application.Sets;

/// <param name="Id">Id набора</param>
public sealed record GetSetQuery(Guid Id) : IRequest<ErrorOr<SetResponse>>;

public sealed class GetSetHandler(ISetRepository repository, ICurrentUser currentUser)
    : IRequestHandler<GetSetQuery, ErrorOr<SetResponse>>
{
    public async Task<ErrorOr<SetResponse>> Handle(GetSetQuery query, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return SetErrors.UserUnresolved;

        var set = await repository.FindOwnedAsync(query.Id, ownerId, cancellationToken);
        return set is null ? SetErrors.NotFound : SetResponse.From(set);
    }
}
