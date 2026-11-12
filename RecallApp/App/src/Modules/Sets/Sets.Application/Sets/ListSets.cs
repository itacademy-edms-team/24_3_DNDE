using Common.Application.Pagination;
using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Sets;

namespace Sets.Application.Sets;

/// <param name="Cursor">Непрозрачный nextCursor предыдущей страницы; null - первая страница.</param>
/// <param name="PageSize">Размер страницы; null - значение по умолчанию.</param>
public sealed record ListSetsQuery(string? Cursor, int? PageSize)
    : IRequest<ErrorOr<KeysetPage<SetSummaryResponse>>>;

public sealed class ListSetsHandler(ISetRepository repository, ICurrentUser currentUser)
    : IRequestHandler<ListSetsQuery, ErrorOr<KeysetPage<SetSummaryResponse>>>
{
    public async Task<ErrorOr<KeysetPage<SetSummaryResponse>>> Handle(
        ListSetsQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return SetErrors.UserUnresolved;

        var pageSize = query.PageSize ?? SetListConstraints.DefaultPageSize;
        if (pageSize is < SetListConstraints.MinPageSize or > SetListConstraints.MaxPageSize)
            return SetErrors.PageSizeInvalid(pageSize);

        SetCursor? cursor = null;
        if (query.Cursor is not null)
        {
            cursor = SetCursor.TryDecode(query.Cursor);
            if (cursor is null)
                return SetErrors.CursorInvalid;
        }

        // Запрашиваем на одну строчку больше, чтобы убедиться, что это не последняя страница
        var rows = await repository.ListOwnedAsync(ownerId, cursor, pageSize + 1, cancellationToken);

        var hasMore = rows.Count > pageSize;
        var items = hasMore ? rows.Take(pageSize).ToList() : rows;
        var nextCursor = hasMore ? new SetCursor(items[^1].CreatedAt, items[^1].Id).Encode() : null;

        return new KeysetPage<SetSummaryResponse>(items.Select(SetSummaryResponse.From).ToList(), nextCursor);
    }
}
