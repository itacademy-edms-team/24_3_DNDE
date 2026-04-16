namespace FinanceTrack.Finance.UseCases.ImportTransactions.ImportFromPdfToNewWallet;

public sealed record ImportFromPdfToNewWalletCommand(
    string UserId,
    string BankStatementType,
    byte[] PdfBytes,
    string WalletName,
    string WalletType,
    bool AllowNegativeBalance,
    decimal? TargetAmount,
    DateOnly? TargetDate
) : ICommand<Result<ImportFromPdfToNewWalletResult>>;
