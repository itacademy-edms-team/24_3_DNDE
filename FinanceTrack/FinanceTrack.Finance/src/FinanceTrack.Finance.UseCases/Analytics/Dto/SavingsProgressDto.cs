namespace FinanceTrack.Finance.UseCases.Analytics.Dto;

public sealed record SavingsProgressDto(IReadOnlyList<SavingsAccountProgressDto> Accounts);

public sealed record SavingsAccountProgressDto(
    Guid WalletId,
    string WalletName,
    decimal Balance,
    decimal TargetAmount,
    DateOnly? TargetDate,
    decimal ProgressPercent
);
