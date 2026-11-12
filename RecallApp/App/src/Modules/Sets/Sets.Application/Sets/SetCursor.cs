using Common.Application.Pagination;

namespace Sets.Application.Sets;

/// <summary>
/// Позиция в списке наборов: сортировка <c>created_at DESC, id DESC</c>,
/// id - tie-breaker для наборов, созданных в одну микросекунду.
/// Передаётся клиенту в непрозрачном виде, разбор - на сервере.
/// </summary>
public sealed record SetCursor(DateTime CreatedAt, Guid Id)
{
    public string Encode() => CursorCodec.Encode(this);

    public static SetCursor? TryDecode(string? cursor) =>
        CursorCodec.TryDecode<SetCursor>(cursor, out var decoded) ? decoded : null;
}
