using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.Infrastructure.PdfImport.Abstractions;

public interface ITransactionsExtractor<out TMatch>
    where TMatch : TransactionMatchBase
{
    IEnumerable<TMatch> Extract(string rawText);
}
