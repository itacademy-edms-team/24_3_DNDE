using System.Globalization;

namespace FinanceTrack.Finance.Infrastructure.PdfImport;

public static class PdfValueParser
{
    public static bool TryParseAmount(string value, out decimal amount)
    {
        // Except different space characters usage
        var normalized = value
            .Replace("\u00A0", "") // non-breaking space
            .Replace("\u2009", "") // thin space
            .Replace(" ", "") // regular space
            .Replace(",", "."); // except different decimal separator usage

        return decimal.TryParse(
            normalized,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out amount
        );
    }

    public static bool TryParseDate(string value, out DateOnly date) =>
        DateOnly.TryParseExact(
            value,
            "dd.MM.yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date
        );
}
