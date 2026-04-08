namespace FinanceTrack.Finance.UseCases.ImportTransactions;

public interface IBankStatementImportService
{
    Result<List<TransactionImportDto>> Import(byte[] pdfBytes, BankStatementType type);
}
