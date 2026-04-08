namespace FinanceTrack.Finance.UseCases.ImportTransactions.ImportFromPdfToNewWallet;

public record ImportFromPdfToNewWalletResult(Guid WalletId, int ImportCount);
