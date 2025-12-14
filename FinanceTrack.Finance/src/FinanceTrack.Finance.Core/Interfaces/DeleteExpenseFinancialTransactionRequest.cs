using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.Core.Interfaces;

public sealed record DeleteExpenseFinancialTransactionRequest(Guid TransactionId, string UserId);
