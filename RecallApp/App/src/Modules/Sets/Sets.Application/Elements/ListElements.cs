using Common.Application.Pagination;
using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Elements;
using Sets.Domain.Sets;

namespace Sets.Application.Elements;

public sealed record ListElementsQuery(Guid SetId, string? Cursor, int? PageSize)
    : IRequest<ErrorOr<KeysetPage<ElementResponse>>>;

public sealed class ListElementsHandler(
    ISetRepository sets,
    IElementRepository elements,
    ICurrentUser currentUser)
    : IRequestHandler<ListElementsQuery, ErrorOr<KeysetPage<ElementResponse>>>
{
    public async Task<ErrorOr<KeysetPage<ElementResponse>>> Handle(
        ListElementsQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return SetErrors.UserUnresolved;

        // Чужой набор неотличим от несуществующего: 404 до любых выборок по элементам.
        if (await sets.FindOwnedAsync(query.SetId, ownerId, cancellationToken) is null)
            return SetErrors.NotFound;

        var pageSize = query.PageSize ?? ElementListConstraints.DefaultPageSize;
        if (pageSize is < ElementListConstraints.MinPageSize or > ElementListConstraints.MaxPageSize)
            return ElementErrors.PageSizeInvalid(pageSize);

        ElementCursor? cursor = null;
        if (query.Cursor is not null)
        {
            cursor = ElementCursor.TryDecode(query.Cursor);
            if (cursor is null)
                return ElementErrors.CursorInvalid;
        }

        var rows = await elements.ListBySetAsync(query.SetId, cursor, pageSize + 1, cancellationToken);

        var hasMore = rows.Count > pageSize;
        var items = hasMore ? rows.Take(pageSize).ToList() : rows;
        var nextCursor = hasMore ? new ElementCursor(items[^1].Position, items[^1].Id).Encode() : null;

        return new KeysetPage<ElementResponse>(items.Select(ElementResponse.From).ToList(), nextCursor);
    }
}
