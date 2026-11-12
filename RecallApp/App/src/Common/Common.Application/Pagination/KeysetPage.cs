namespace Common.Application.Pagination;

/// <summary>
/// Страница при курсорной пагинации.
/// <see cref="NextCursor"/> = null означает, что представленная страница - последняя.
/// </summary>
/// <typeparam name="TItem">Тип элемента страницы.</typeparam>
public sealed record KeysetPage<TItem>(IReadOnlyList<TItem> Items, string? NextCursor);
