using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.Core.Interfaces;

public interface ICreateExpenseFinancialTransactionService
{
    public Task<Result<Guid>> CreateExpenseFinancialTransaction(
        CreateExpenseFinancialTransactionRequest request,
        CancellationToken cancellationToken = default
    );
}
