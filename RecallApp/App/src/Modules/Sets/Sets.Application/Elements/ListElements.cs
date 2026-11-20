using Common.Application.Pagination;
using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Media.Contracts;
using Sets.Application.Abstractions;
using Sets.Domain.Elements;
using Sets.Domain.Sets;

namespace Sets.Application.Elements;

/// <summary>
/// Возвращает страницу элементов набора.
/// </summary>
/// <param name="SetId">Идентификатор набора.</param>
/// <param name="Cursor">Непрозрачный курсор предыдущей страницы; <see langword="null"/> - первая страница.</param>
/// <param name="PageSize">Размер страницы; <see langword="null"/> - значение по умолчанию.</param>
/// <remarks>
/// <para>Порядок - по позициям (рангам), затем по идентификатору (ADR 0039).</para>
/// <para>Чужой набор неотличим от несуществующего: на обе проблемы
/// возвращается ошибка <see cref="SetErrors.NotFound"/>.</para>
/// <para>Битый курсор и размер страницы вне допустимого диапазона возвращают
/// <see cref="ElementErrors.CursorInvalid"/> и <see cref="ElementErrors.PageSizeInvalid"/>.</para>
/// <para>Ссылки на изображения подписываются пачкой на страницу, поэтому поле <c>images</c>
/// заполнено только у элементов с готовыми файлами.</para>
/// </remarks>
public sealed record ListElementsQuery(Guid SetId, string? Cursor, int? PageSize)
    : IRequest<ErrorOr<KeysetPage<ElementResponse>>>;

public sealed class ListElementsHandler(
    ISetRepository sets,
    IElementRepository elements,
    IMediaLinksQuery mediaLinks,
    ICurrentUser currentUser)
    : IRequestHandler<ListElementsQuery, ErrorOr<KeysetPage<ElementResponse>>>
{
    public async Task<ErrorOr<KeysetPage<ElementResponse>>> Handle(
        ListElementsQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return SetErrors.UserUnresolved;

        // Чужой набор неотличим от несуществующего. Возвращаем одинаковый код ошибки.
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

        // Ссылки на изображения подписываются пачкой: один вызов на страницу, не на элемент.
        var imageIds = items
            .SelectMany(item => ElementMediaSlots.ImageIds(item.Payload))
            .Distinct()
            .ToArray();

        var links = await mediaLinks.GetLinksAsync(ownerId, imageIds, cancellationToken);

        return new KeysetPage<ElementResponse>(
            items.Select(item => ElementResponse.From(item, links)).ToList(),
            nextCursor);
    }
}
