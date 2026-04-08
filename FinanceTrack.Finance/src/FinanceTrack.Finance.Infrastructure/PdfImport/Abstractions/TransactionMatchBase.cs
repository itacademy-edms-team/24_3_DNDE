using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.Infrastructure.PdfImport.Abstractions;

public abstract class TransactionMatchBase
{
    public abstract string OperationDate { get; init; }
    public abstract string TransferDate { get; init; }
    public abstract string IncomeAmount { get; init; }
    public abstract string ExpenseAmount { get; init; }
    public abstract string Description { get; init; }
}
