using Common.Application.Pagination;

namespace Sets.Application.Elements;

/// <summary>
/// Представляет курсор постраничной выдачи элементов набора.
/// </summary>
/// <remarks>
/// Хранит ключ сортировки <c>position ASC, id ASC</c> (ADR 0039).
/// Идентификатор используется как tie-breaker на случай совпадения ранга.
/// Ранг сравнивается побайтово (<c>COLLATE "C"</c>), поэтому порядок в базе совпадает с порядком,
/// который вычисляет генератор. Клиент получает курсор непрозрачно.
/// </remarks>
/// <param name="Position">Позиция последнего выданного элемента (строковый ранг).</param>
/// <param name="Id">Идентификатор последнего выданного элемента.</param>
public sealed record ElementCursor(string Position, Guid Id)
{
    /// <summary>
    /// Кодирует курсор в непрозрачную строку.
    /// </summary>
    /// <returns>Строка курсора.</returns>
    public string Encode() => CursorCodec.Encode(this);

    /// <summary>
    /// Разбирает курсор и проверяет его содержимое.
    /// </summary>
    /// <param name="cursor">Непрозрачная строка от клиента.</param>
    /// <returns>
    /// Курсор или <see langword="null"/>, если строка повреждена, поля отсутствуют или ранг пуст.
    /// </returns>
    public static ElementCursor? TryDecode(string? cursor)
    {
        if (!CursorCodec.TryDecode<ElementCursor>(cursor, out var decoded) || decoded is null)
            return null;

        return decoded.IsValid ? decoded : null;
    }

    private bool IsValid => !string.IsNullOrWhiteSpace(Position) && Id != Guid.Empty;
}
