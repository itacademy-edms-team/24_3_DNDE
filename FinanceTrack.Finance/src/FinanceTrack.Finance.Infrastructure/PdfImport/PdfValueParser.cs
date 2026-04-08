using System.Globalization;

namespace FinanceTrack.Finance.Infrastructure.PdfImport;

public static class PdfValueParser
{
    public static decimal ParseAmount(string value)
    {
        // Except different space characters usage
        var normalized = value
            .Replace("\u00A0", "") // non-breaking space
            .Replace("\u2009", "") // thin space
            .Replace(" ", "") // regular space
            .Replace(",", "."); // except different decimal separator usage

        return decimal.Parse(normalized, CultureInfo.InvariantCulture);
    }

    public static DateOnly ParseDate(string value) => DateOnly.ParseExact(value, "dd.MM.yyyy");
}
