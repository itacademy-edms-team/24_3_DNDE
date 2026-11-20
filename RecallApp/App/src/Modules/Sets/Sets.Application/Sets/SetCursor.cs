using Common.Application.Pagination;

namespace Sets.Application.Sets;

/// <summary>
/// Представляет курсор постраничной выдачи списка наборов.
/// </summary>
/// <remarks>
/// Хранит ключ сортировки <c>created_at DESC, id DESC</c> (ADR 0039).
/// Идентификатор нужен как tie-breaker для наборов, созданных в одну микросекунду.
/// Клиент получает курсор непрозрачно и не занимается его разбором.
/// </remarks>
public sealed record SetCursor(DateTime CreatedAt, Guid Id)
{
    /// <summary>
    /// Кодирует курсор в непрозрачную строку для клиента.
    /// </summary>
    /// <returns>Строка курсора.</returns>
    public string Encode() => CursorCodec.Encode(this);

    /// <summary>
    /// Разбирает курсор и проверяет его содержимое.
    /// </summary>
    /// <param name="cursor">Непрозрачная строка от клиента.</param>
    /// <returns>Курсор или <see langword="null"/>, если строка повреждена, обязательные поля
    /// отсутствуют или время в ней без метки UTC.</returns>
    /// <remarks>
    /// Возвращается <see langword="null"/>, а не пустая страница:
    /// чтобы отличить отсутствие данных от испорченного курсора.
    /// </remarks>
    public static SetCursor? TryDecode(string? cursor)
    {
        if (!CursorCodec.TryDecode<SetCursor>(cursor, out var decoded) || decoded is null)
            return null;

        return decoded.IsValid ? decoded : null;
    }

    private bool IsValid =>
        Id != Guid.Empty
        && CreatedAt != default
        && CreatedAt.Kind == DateTimeKind.Utc;
}
