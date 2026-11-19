using Sets.Domain.Sets;

namespace Sets.Application.Sets;

/// <summary>
/// Представляет карточку набора в списке «Мои наборы».
/// </summary>
/// <param name="Id">Идентификатор набора.</param>
/// <param name="Title">Название.</param>
/// <param name="Description">Описание; пустая строка означает, что описания нет.</param>
/// <param name="CreatedAt">Момент создания в UTC.</param>
/// <param name="UpdatedAt">Момент последнего изменения в UTC.</param>
public sealed record SetSummaryResponse(
    Guid Id,
    string Title,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    /// <summary>
    /// Создаёт карточку набора по доменному набору.
    /// </summary>
    /// <param name="set">Доменный набор.</param>
    /// <returns>Карточка для списка.</returns>
    public static SetSummaryResponse From(Set set) =>
        new(set.Id, set.Title.Value, set.Description.Value, set.CreatedAt, set.UpdatedAt);
}

/// <summary>
/// Представляет набор без элементов; число элементов приходит в <see cref="ElementCount"/>.
/// </summary>
/// <param name="Id">Идентификатор набора.</param>
/// <param name="Title">Название.</param>
/// <param name="Description">Описание; пустая строка означает, что описания нет.</param>
/// <param name="CreatedAt">Момент создания в UTC.</param>
/// <param name="UpdatedAt">Момент последнего изменения в UTC.</param>
/// <param name="ElementCount">Число активных элементов набора.</param>
public sealed record SetResponse(
    Guid Id,
    string Title,
    string Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int ElementCount)
{
    /// <summary>
    /// Создаёт представление набора с числом активных элементов.
    /// </summary>
    /// <param name="set">Доменный набор.</param>
    /// <param name="elementCount">Число активных элементов, посчитанное отдельно.</param>
    /// <returns>Представление набора для ответа API.</returns>
    public static SetResponse From(Set set, int elementCount) =>
        new(set.Id, set.Title.Value, set.Description.Value, set.CreatedAt, set.UpdatedAt, elementCount);
}
