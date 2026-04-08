using Ardalis.Result;
using FinanceTrack.Finance.UseCases.ImportTransactions;

namespace FinanceTrack.Finance.Infrastructure.PdfImport;

public class BankStatementImportService : IBankStatementImportService
{
    public Result<List<TransactionImportDto>> Import(byte[] pdfBytes, BankStatementType type)
    {
        if (!PdfValidator.IsPdf(pdfBytes))
            return Result<List<TransactionImportDto>>.Error("File is not a valid PDF.");

        var rawText = PdfTextExtractor.Extract(pdfBytes);
        var extractor = ExtractorFactory.GetExtractor(type);
        var dtos = extractor.Extract(rawText).Select(FinancialTransactionMapper.ToDto).ToList();

        if (!dtos.Any())
            return Result<List<TransactionImportDto>>.Error(
                "No transactions found. Make sure the file matches the selected bank statement type."
            );

        return Result.Success(dtos);
    }
}
