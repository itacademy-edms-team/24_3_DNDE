using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FinanceTrack.Finance.Infrastructure.PdfImport.Abstractions;

public abstract class TextExtractorBase<TMatch>
    where TMatch : TransactionMatchBase
{
    protected Regex TransactionRegex { get; }

    protected TextExtractorBase(string pattern)
    {
        TransactionRegex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Multiline);
    }

    public abstract IEnumerable<TMatch> Extract(string rawText);
}
