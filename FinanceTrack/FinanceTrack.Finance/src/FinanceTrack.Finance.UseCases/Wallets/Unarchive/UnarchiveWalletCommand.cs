namespace FinanceTrack.Finance.UseCases.Wallets.Unarchive;

public sealed record UnarchiveWalletCommand(Guid WalletId, string UserId) : ICommand<Result>;
