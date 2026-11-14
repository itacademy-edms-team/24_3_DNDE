using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Sets;

namespace Sets.Application.Sets;

/// <param name="Id">Id набора</param>
public sealed record GetSetQuery(Guid Id) : IRequest<ErrorOr<SetResponse>>;

public sealed class GetSetHandler(
    ISetRepository repository,
    IElementRepository elements,
    ICurrentUser currentUser)
    : IRequestHandler<GetSetQuery, ErrorOr<SetResponse>>
{
    public async Task<ErrorOr<SetResponse>> Handle(GetSetQuery query, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return SetErrors.UserUnresolved;

        var set = await repository.FindOwnedAsync(query.Id, ownerId, cancellationToken);
        if (set is null)
            return SetErrors.NotFound;

        var elementCount = await elements.CountActiveAsync(set.Id, cancellationToken);
        return SetResponse.From(set, elementCount);
    }
}
