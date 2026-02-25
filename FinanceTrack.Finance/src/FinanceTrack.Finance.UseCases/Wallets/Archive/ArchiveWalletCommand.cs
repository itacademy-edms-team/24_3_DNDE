namespace FinanceTrack.Finance.UseCases.Wallets.Archive;

public sealed record ArchiveWalletCommand(Guid WalletId, string UserId) : ICommand<Result>;
