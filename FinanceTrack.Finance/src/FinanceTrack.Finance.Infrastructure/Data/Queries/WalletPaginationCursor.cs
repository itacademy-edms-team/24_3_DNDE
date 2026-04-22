using System.Globalization;
using System.Text;

namespace FinanceTrack.Finance.Infrastructure.Data.Queries;

internal static class WalletPaginationCursor
{
    public static string Encode(DateTime createdAtUtc, Guid id)
    {
        var raw = $"{createdAtUtc:O}|{id}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    public static bool TryDecode(string cursor, out DateTime createdAtUtc, out Guid id)
    {
        createdAtUtc = default;
        id = default;

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = raw.Split('|');
            if (parts.Length != 2)
                return false;

            if (!DateTime.TryParse(parts[0], null, DateTimeStyles.RoundtripKind, out createdAtUtc))
                return false;
            if (!Guid.TryParse(parts[1], out id))
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }
}
