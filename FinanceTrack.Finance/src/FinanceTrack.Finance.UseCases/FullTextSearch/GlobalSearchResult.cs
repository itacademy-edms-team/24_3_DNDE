using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceTrack.Finance.UseCases.FinancialTransactions;
using FinanceTrack.Finance.UseCases.RecurringTransactions;
using FinanceTrack.Finance.UseCases.Wallets;

namespace FinanceTrack.Finance.UseCases.FullTextSearch;

public sealed record GlobalSearchResult(
    IReadOnlyList<WalletDto> Wallets,
    IReadOnlyList<FinancialTransactionDto> Incomes,
    IReadOnlyList<FinancialTransactionDto> Expenses,
    IReadOnlyList<FinancialTransactionDto> Transfers,
    IReadOnlyList<RecurringTransactionDto> RecurringTransactions
);
