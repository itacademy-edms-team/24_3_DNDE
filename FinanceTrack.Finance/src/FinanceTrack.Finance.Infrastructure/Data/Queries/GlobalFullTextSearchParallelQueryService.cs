using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Result;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Infrastructure.Data.Config;
using FinanceTrack.Finance.UseCases.FinancialTransactions;
using FinanceTrack.Finance.UseCases.FullTextSearch;
using FinanceTrack.Finance.UseCases.RecurringTransactions;
using FinanceTrack.Finance.UseCases.Wallets;
using NpgsqlTypes;

namespace FinanceTrack.Finance.Infrastructure.Data.Queries;

public class GlobalFullTextSearchParallelQueryService(IServiceScopeFactory serviceScopeFactory)
    : IGlobalFullTextSearchQueryService
{
    public async Task<Result<GlobalSearchResult>> SearchAsync(
        string userId,
        string userQuery,
        int limitPerType,
        CancellationToken ct
    )
    {
        var walletsTask = QueryWalletsAsync(userId, userQuery, limitPerType, ct);

        var incomesTask = QueryIncomesAsync(userId, userQuery, limitPerType, ct);

        var expensesTask = QueryExpensesAsync(userId, userQuery, limitPerType, ct);

        var transfersInTask = QueryTransfersInAsync(userId, userQuery, limitPerType, ct);

        var recurringsTask = QueryRecurringTransactionsAsync(userId, userQuery, limitPerType, ct);

        await Task.WhenAll(walletsTask, incomesTask, expensesTask, transfersInTask, recurringsTask);

        var result = new GlobalSearchResult(
            walletsTask.Result,
            incomesTask.Result,
            expensesTask.Result,
            transfersInTask.Result,
            recurringsTask.Result
        );

        return Result.Success(result);
    }

    private async Task<IReadOnlyList<WalletDto>> QueryWalletsAsync(
        string userId,
        string userQuery,
        int limitPerType,
        CancellationToken ct
    )
    {
        using var scope = serviceScopeFactory.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await appDbContext
            .Wallets.Where(w =>
                w.UserId == userId
                && EF.Property<NpgsqlTsVector>(w, DbConstants.FullTextSearchPropertyName)
                    .Matches(EF.Functions.WebSearchToTsQuery(userQuery))
            )
            .OrderByDescending(w =>
                EF.Property<NpgsqlTsVector>(w, DbConstants.FullTextSearchPropertyName)
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
    }

    private async Task<IReadOnlyList<FinancialTransactionDto>> QueryIncomesAsync(
        string userId,
        string userQuery,
        int limitPerType,
        CancellationToken ct
    )
    {
        using var scope = serviceScopeFactory.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await appDbContext
            .FinancialTransactions.Where(f =>
                f.UserId == userId
                && f.TransactionType == FinancialTransactionType.Income
                && EF.Property<NpgsqlTsVector>(f, DbConstants.FullTextSearchPropertyName)
                    .Matches(EF.Functions.WebSearchToTsQuery(userQuery))
            )
            .OrderByDescending(f =>
                EF.Property<NpgsqlTsVector>(f, DbConstants.FullTextSearchPropertyName)
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
    }

    private async Task<IReadOnlyList<FinancialTransactionDto>> QueryExpensesAsync(
        string userId,
        string userQuery,
        int limitPerType,
        CancellationToken ct
    )
    {
        using var scope = serviceScopeFactory.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await appDbContext
            .FinancialTransactions.Where(f =>
                f.UserId == userId
                && f.TransactionType == FinancialTransactionType.Expense
                && EF.Property<NpgsqlTsVector>(f, DbConstants.FullTextSearchPropertyName)
                    .Matches(EF.Functions.WebSearchToTsQuery(userQuery))
            )
            .OrderByDescending(f =>
                EF.Property<NpgsqlTsVector>(f, DbConstants.FullTextSearchPropertyName)
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
    }

    private async Task<IReadOnlyList<FinancialTransactionDto>> QueryTransfersInAsync(
        string userId,
        string userQuery,
        int limitPerType,
        CancellationToken ct
    )
    {
        using var scope = serviceScopeFactory.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await appDbContext
            .FinancialTransactions.Where(f =>
                f.UserId == userId
                && f.TransactionType == FinancialTransactionType.TransferIn
                && EF.Property<NpgsqlTsVector>(f, DbConstants.FullTextSearchPropertyName)
                    .Matches(EF.Functions.WebSearchToTsQuery(userQuery))
            )
            .OrderByDescending(f =>
                EF.Property<NpgsqlTsVector>(f, DbConstants.FullTextSearchPropertyName)
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
    }

    private async Task<IReadOnlyList<RecurringTransactionDto>> QueryRecurringTransactionsAsync(
        string userId,
        string userQuery,
        int limitPerType,
        CancellationToken ct
    )
    {
        using var scope = serviceScopeFactory.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await appDbContext
            .RecurringTransactions.Where(r =>
                r.UserId == userId
                && EF.Property<NpgsqlTsVector>(r, DbConstants.FullTextSearchPropertyName)
                    .Matches(EF.Functions.WebSearchToTsQuery(userQuery))
            )
            .OrderByDescending(r =>
                EF.Property<NpgsqlTsVector>(r, DbConstants.FullTextSearchPropertyName)
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
    }
}
