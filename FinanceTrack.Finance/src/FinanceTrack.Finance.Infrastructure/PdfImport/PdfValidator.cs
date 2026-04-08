namespace FinanceTrack.Finance.Infrastructure.PdfImport;

internal static class PdfValidator
{
    private static readonly byte[] PdfMagicBytes = "%PDF"u8.ToArray();

    public static bool IsPdf(byte[] bytes) =>
        bytes.Length >= PdfMagicBytes.Length
        && bytes.AsSpan(0, PdfMagicBytes.Length).SequenceEqual(PdfMagicBytes);
}
