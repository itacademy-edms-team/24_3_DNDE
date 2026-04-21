using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Core.WalletAggregate;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FinanceTrack.Finance.UseCases.FinancialTransactions;

public sealed record FinancialTransactionDto(
    Guid Id,
    Guid WalletId,
    string Name,
    string? Description,
    decimal Amount,
    DateOnly OperationDate,
    string Type,
    Guid? CategoryId,
    Guid? RelatedTransactionId,
    Guid? RecurringTransactionId,
    Guid? RelatedWalletId = null,
    string? RelatedWalletName = null,
    string? WalletName = null
)
{
    public static FinancialTransactionDto FromEntity(
        FinancialTransaction t,
        Dictionary<Guid, FinancialTransaction> relatedTransactions,
        Dictionary<Guid, Wallet> relatedWallets
    )
    {
        Guid? relatedWalletId = null;
        string? relatedWalletName = null;

        if (
            t.RelatedTransactionId.HasValue
            && relatedTransactions.TryGetValue(
                t.RelatedTransactionId.Value,
                out var relatedTransaction
            )
        )
        {
            relatedWalletId = relatedTransaction.WalletId;
            if (relatedWallets.TryGetValue(relatedTransaction.WalletId, out var relatedWallet))
                relatedWalletName = relatedWallet.Name;
        }

        return new FinancialTransactionDto(
            t.Id,
            t.WalletId,
            t.Name,
            t.Description,
            t.Amount,
            t.OperationDate,
            t.TransactionType.Name,
            t.CategoryId,
            t.RelatedTransactionId,
            t.RecurringTransactionId,
            relatedWalletId,
            relatedWalletName
        );
    }
};
