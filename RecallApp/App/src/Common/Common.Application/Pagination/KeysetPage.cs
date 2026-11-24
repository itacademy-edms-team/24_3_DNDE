namespace Common.Application.Pagination;

/// <summary>
/// Представляет страницу при курсорной пагинации.
/// </summary>
/// <typeparam name="TItem">Тип элемента страницы.</typeparam>
/// <param name="Items">Элементы страницы.</param>
/// <param name="NextCursor">Курсор следующей страницы; <see langword="null"/> означает, что эта
/// страница последняя.</param>
public sealed record KeysetPage<TItem>(IReadOnlyList<TItem> Items, string? NextCursor);
