namespace FinanceTrack.Finance.UseCases.Wallets.List;

public sealed record WalletPageDto(
    IReadOnlyList<WalletDto> Wallets,
    string? NextCursor,
    bool HasMore
);
