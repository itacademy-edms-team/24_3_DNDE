using Common.Application.Pagination;

namespace Sets.Application.Sets;

/// <summary>
/// Курсор постраничной выдачи списка наборов (ADR 0039).
/// Хранит ключ сортировки <c>created_at DESC, id DESC</c>: id - tie-breaker
/// для наборов, созданных в одну микросекунду. Клиент возвращает его непрозрачно.
/// </summary>
public sealed record SetCursor(DateTime CreatedAt, Guid Id)
{
    public string Encode() => CursorCodec.Encode(this);

    /// <summary>
    /// Разбирает курсор и проверяет содержимое: битый JSON, отсутствующие поля или
    /// время без UTC-метки дают null, а не пустую страницу с «данных больше нет».
    /// </summary>
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
