using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.Core.Interfaces;

public interface ICreateIncomeFinancialTransactionService
{
    Task<Result<Guid>> AddIncome(
        CreateIncomeFinancialTransactionRequest request,
        CancellationToken ct
    );
}
