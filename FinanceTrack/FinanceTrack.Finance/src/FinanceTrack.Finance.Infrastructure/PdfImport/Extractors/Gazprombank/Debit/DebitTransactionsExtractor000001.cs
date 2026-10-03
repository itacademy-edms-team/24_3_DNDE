using FinanceTrack.Finance.Infrastructure.PdfImport.Abstractions;

namespace FinanceTrack.Finance.Infrastructure.PdfImport.Extractors.Gazprombank.Debit;

public class DebitTransactionsExtractor000001
    : TextExtractorBase<DebitTransactionMatch000001>,
        ITransactionsExtractor<DebitTransactionMatch000001>
{
    public const string Pattern =
        @"(?<operation_debit_date>\d{2}\.\d{2}\.\d{4})\s(?<operation_transfer_date>\d{2}\.\d{2}\.\d{4})\s(?<operation_description>[\s\S]+?)(?=\s*[+\-]\d[\d\s]*,\d{2})\s[+](?<operation_income_value_rub>\d[\d\s]*,\d{2})\s[-](?<operation_expense_value_rub>\d[\d\s]*,\d{2})";

    public DebitTransactionsExtractor000001()
        : base(Pattern) { }

    public override IEnumerable<DebitTransactionMatch000001> Extract(string rawText) =>
        TransactionRegex.Matches(rawText).Select(DebitTransactionMatch000001.FromMatch);
}
