using Common.Application.Pagination;

namespace Sets.Application.Elements;

/// <summary>
/// Курсор постраничной выдачи элементов набора (ADR 0039).
/// Хранит ключ сортировки <c>position ASC, id ASC</c>:
/// id - tie-breaker на случай рассинхронизации. Клиент возвращает его непрозрачно.
/// </summary>
public sealed record ElementCursor(int Position, Guid Id)
{
    public string Encode() => CursorCodec.Encode(this);

    public static ElementCursor? TryDecode(string? cursor)
    {
        if (!CursorCodec.TryDecode<ElementCursor>(cursor, out var decoded) || decoded is null)
            return null;

        return decoded.IsValid ? decoded : null;
    }

    private bool IsValid => Position >= 0 && Id != Guid.Empty;
}
