using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Result;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.UseCases.FinancialTransactions;
using FinanceTrack.Finance.UseCases.FullTextSearch;
using FinanceTrack.Finance.UseCases.RecurringTransactions;
using FinanceTrack.Finance.UseCases.Wallets;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace FinanceTrack.Finance.Infrastructure.Data.Queries;

public class GlobalFullTextSearchQueryService(AppDbContext appDbContext)
    : IGlobalFullTextSearchQueryService
{
    public async Task<Result<GlobalSearchResult>> SearchAsync(
        string userId,
        string userQuery,
        int limitPerType,
        CancellationToken ct
    )
    {
        var wallets = await appDbContext
            .Wallets.Where(w =>
                w.UserId == userId
                && EF.Functions.ToTsVector("russian", w.Name)
                    .Matches(EF.Functions.WebSearchToTsQuery(userQuery))
            )
            .OrderByDescending(w =>
                EF.Property<NpgsqlTsVector>(w, "SearchVector")
                    .Rank(EF.Functions.WebSearchToTsQuery(userQuery))
            )
            .Take(limitPerType)
            .Select(w => new WalletDto(
                w.Id,
                w.Name,
                w.WalletType.Name,
                w.Balance,
                w.AllowNegativeBalance,
                w.TargetAmount,
                w.TargetDate,
                w.IsArchived
            ))
            .ToListAsync(ct);

        var incomes = await appDbContext
            .FinancialTransactions.Where(f =>
                f.UserId == userId
                && f.TransactionType == FinancialTransactionType.Income
                && EF.Functions.ToTsVector("russian", f.Name)
                    .Matches(EF.Functions.WebSearchToTsQuery(userQuery))
            )
            .OrderByDescending(f =>
                EF.Property<NpgsqlTsVector>(f, "SearchVector")
                    .Rank(EF.Functions.WebSearchToTsQuery(userQuery))
            )
            .Take(limitPerType)
            .Select(f => new FinancialTransactionDto(
                f.Id,
                f.WalletId,
                f.Name,
                f.Description,
                f.Amount,
                f.OperationDate,
                f.TransactionType.Name,
                f.CategoryId,
                f.RelatedTransactionId,
                f.RecurringTransactionId,
                null,
                null
            ))
            .ToListAsync(ct);

        var expenses = await appDbContext
            .FinancialTransactions.Where(f =>
                f.UserId == userId
                && f.TransactionType == FinancialTransactionType.Expense
                && EF.Functions.ToTsVector("russian", f.Name)
                    .Matches(EF.Functions.WebSearchToTsQuery(userQuery))
            )
            .OrderByDescending(f =>
                EF.Property<NpgsqlTsVector>(f, "SearchVector")
                    .Rank(EF.Functions.WebSearchToTsQuery(userQuery))
            )
            .Take(limitPerType)
            .Select(f => new FinancialTransactionDto(
                f.Id,
                f.WalletId,
                f.Name,
                f.Description,
                f.Amount,
                f.OperationDate,
                f.TransactionType.Name,
                f.CategoryId,
                f.RelatedTransactionId,
                f.RecurringTransactionId,
                null,
                null
            ))
            .ToListAsync(ct);

        var transfers = await appDbContext
            .FinancialTransactions.Where(f =>
                f.UserId == userId
                && f.TransactionType == FinancialTransactionType.TransferIn
                && EF.Functions.ToTsVector("russian", f.Name)
                    .Matches(EF.Functions.WebSearchToTsQuery(userQuery))
            )
            .OrderByDescending(f =>
                EF.Property<NpgsqlTsVector>(f, "SearchVector")
                    .Rank(EF.Functions.WebSearchToTsQuery(userQuery))
            )
            .Take(limitPerType)
            .Select(f => new FinancialTransactionDto(
                f.Id,
                f.WalletId,
                f.Name,
                f.Description,
                f.Amount,
                f.OperationDate,
                f.TransactionType.Name,
                f.CategoryId,
                f.RelatedTransactionId,
                f.RecurringTransactionId,
                null,
                null
            ))
            .ToListAsync(ct);

        var recurringTransactions = await appDbContext
            .RecurringTransactions.Where(r =>
                r.UserId == userId
                && EF.Functions.ToTsVector("russian", r.Name)
                    .Matches(EF.Functions.WebSearchToTsQuery(userQuery))
            )
            .OrderByDescending(r =>
                EF.Property<NpgsqlTsVector>(r, "SearchVector")
                    .Rank(EF.Functions.WebSearchToTsQuery(userQuery))
            )
            .Take(limitPerType)
            .Select(r => new RecurringTransactionDto(
                r.Id,
                r.WalletId,
                r.CategoryId,
                r.Name,
                r.Description,
                r.TransactionType.Name,
                r.Amount,
                r.DayOfMonth,
                r.StartDate,
                r.EndDate,
                r.IsActive,
                r.LastProcessedDate
            ))
            .ToListAsync(ct);

        return Result.Success(
            new GlobalSearchResult(wallets, incomes, expenses, transfers, recurringTransactions)
        );
    }
}
