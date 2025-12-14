using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceTrack.Finance.Core.FinancialTransactionAggregate;

namespace FinanceTrack.Finance.Core.Interfaces;

public interface IUpdateExpenseFinancialTransactionService
{
    Task<Result<FinancialTransaction>> UpdateExpenseFinancialTransaction(
        UpdateExpenseFinancialTransactionRequest request,
        CancellationToken cancellationToken = default
    );
}
