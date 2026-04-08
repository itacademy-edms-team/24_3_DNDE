using System.Text;
using Docnet.Core;
using Docnet.Core.Models;

namespace FinanceTrack.Finance.Infrastructure.PdfImport;

internal static class PdfTextExtractor
{
    public static string Extract(byte[] pdfBytes)
    {
        var textBuilder = new StringBuilder();

        using var docReader = DocLib.Instance.GetDocReader(pdfBytes, new PageDimensions(1, 1));
        var pageCount = docReader.GetPageCount();

        for (int i = 0; i < pageCount; i++)
        {
            using var pageReader = docReader.GetPageReader(i);
            textBuilder.AppendLine(pageReader.GetText());
        }

        return textBuilder.ToString();
    }
}
