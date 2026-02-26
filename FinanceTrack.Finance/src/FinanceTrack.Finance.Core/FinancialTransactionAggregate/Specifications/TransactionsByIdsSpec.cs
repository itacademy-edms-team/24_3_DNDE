using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.Core.FinancialTransactionAggregate.Specifications;

public class TransactionsByIdsSpec : Specification<FinancialTransaction>
{
    public TransactionsByIdsSpec(IEnumerable<Guid> transactionIds)
    {
        Query.Where(t => transactionIds.Contains(t.Id));
    }
}
