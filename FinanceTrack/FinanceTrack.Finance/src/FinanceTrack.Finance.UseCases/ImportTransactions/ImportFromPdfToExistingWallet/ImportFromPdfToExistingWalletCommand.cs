namespace FinanceTrack.Finance.UseCases.ImportTransactions.ImportFromPdfToExistingWallet;

public sealed record ImportFromPdfToExistingWalletCommand(
    string UserId,
    Guid WalletId,
    string BankStatementType,
    byte[] PdfBytes
) : ICommand<Result<int>>;
