using Common.Application.Pagination;

namespace Sets.Application.Elements;

/// <summary>
/// Курсор постраничной выдачи элементов набора (ADR 0039).
/// Хранит ключ сортировки <c>position ASC, id ASC</c>:
/// id - tie-breaker на случай совпадения ранга. Клиент возвращает его непрозрачно.
/// </summary>
/// <param name="Position">Строковый ранг последнего выданного элемента.</param>
/// <param name="Id">Идентификатор последнего выданного элемента.</param>
public sealed record ElementCursor(string Position, Guid Id)
{
    /// <summary>
    /// Кодирует курсор в непрозрачную строку.
    /// </summary>
    public string Encode() => CursorCodec.Encode(this);

    /// <summary>
    /// Разбирает курсор и проверяет содержимое.
    /// </summary>
    /// <param name="cursor">Непрозрачная строка от клиента.</param>
    /// <returns>Курсор или <see langword="null"/>, если строку не удалось разобрать.</returns>
    public static ElementCursor? TryDecode(string? cursor)
    {
        if (!CursorCodec.TryDecode<ElementCursor>(cursor, out var decoded) || decoded is null)
            return null;

        return decoded.IsValid ? decoded : null;
    }

    private bool IsValid => !string.IsNullOrWhiteSpace(Position) && Id != Guid.Empty;
}
