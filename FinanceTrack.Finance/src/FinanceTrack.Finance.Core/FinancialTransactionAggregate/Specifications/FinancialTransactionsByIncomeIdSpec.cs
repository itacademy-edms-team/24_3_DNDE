using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;

public class FinancialTransactionsByIncomeIdSpec : Specification<FinancialTransaction>
{
    public FinancialTransactionsByIncomeIdSpec(Guid incomeId)
    {
        Query.Where(e => e.IncomeTransactionId == incomeId);
    }
}
