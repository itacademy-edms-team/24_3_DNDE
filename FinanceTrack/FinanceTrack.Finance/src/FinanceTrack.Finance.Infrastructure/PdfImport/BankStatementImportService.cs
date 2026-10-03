using Ardalis.Result;
using FinanceTrack.Finance.UseCases.ImportTransactions;
using Microsoft.Extensions.Logging;

namespace FinanceTrack.Finance.Infrastructure.PdfImport;

public class BankStatementImportService(ILogger<BankStatementImportService> logger)
    : IBankStatementImportService
{
    public Result<List<TransactionImportDto>> Import(byte[] pdfBytes, BankStatementType type)
    {
        try
        {
            if (!PdfValidator.IsPdf(pdfBytes))
                return Result<List<TransactionImportDto>>.Error("File is not a valid PDF.");

            var rawText = PdfTextExtractor.Extract(pdfBytes);
            var extractor = ExtractorFactory.GetExtractor(type);
            var matches = extractor.Extract(rawText).ToList();

            if (!matches.Any())
                return Result<List<TransactionImportDto>>.Error(
                    "No transactions found. Make sure the file matches the selected bank statement type."
                );

            var dtos = new List<TransactionImportDto>(matches.Count);
            for (var i = 0; i < matches.Count; i++)
            {
                var mapResult = FinancialTransactionMapper.ToDto(matches[i]);
                if (!mapResult.IsSuccess)
                    return Result<List<TransactionImportDto>>.Error(
                        $"Failed to parse transaction #{i + 1}: {mapResult.Errors.FirstOrDefault() ?? "Unknown parse error."}"
                    );

                dtos.Add(mapResult.Value);
            }

            return Result.Success(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return Result<List<TransactionImportDto>>.Error(
                "Failed to parse PDF. File content is invalid or unsupported for the selected statement type."
            );
        }
    }
}
