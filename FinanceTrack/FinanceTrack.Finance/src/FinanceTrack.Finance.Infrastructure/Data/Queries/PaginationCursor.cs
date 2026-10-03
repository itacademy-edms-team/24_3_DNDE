using System.Globalization;
using System.Text;

namespace FinanceTrack.Finance.Infrastructure.Data.Queries;

internal static class PaginationCursor
{
    public static string Encode(DateOnly operationDate, DateTime createdAtUtc, Guid id)
    {
        var raw = $"{operationDate:yyyy-MM-dd}|{createdAtUtc:O}|{id}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    public static bool TryDecode(
        string cursor,
        out DateOnly operationDate,
        out DateTime createdAtUtc,
        out Guid id
    )
    {
        operationDate = default;
        createdAtUtc = default;
        id = default;

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = raw.Split('|');
            if (parts.Length != 3)
                return false;

            if (!DateOnly.TryParseExact(parts[0], "yyyy-MM-dd", out operationDate))
                return false;
            if (
                !DateTime.TryParse(
                    parts[1],
                    null,
                    DateTimeStyles.RoundtripKind,
                    out createdAtUtc
                )
            )
                return false;
            if (!Guid.TryParse(parts[2], out id))
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }
}
