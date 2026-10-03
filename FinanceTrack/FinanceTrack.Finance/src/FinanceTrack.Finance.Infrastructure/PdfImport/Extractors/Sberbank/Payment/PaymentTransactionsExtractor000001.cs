using FinanceTrack.Finance.Infrastructure.PdfImport.Abstractions;

namespace FinanceTrack.Finance.Infrastructure.PdfImport.Extractors.Sberbank.Payment;

public class PaymentTransactionsExtractor000001
    : TextExtractorBase<PaymentTransactionMatch000001>,
        ITransactionsExtractor<PaymentTransactionMatch000001>
{
    public const string Pattern =
        @"(?<operation_debit_date>\d{2}\.\d{2}\.\d{4})\s(?<operation_time>(?>[01][0-9]|2[0-3]):[0-5][0-9])\s+(?<operation_category>.+?)(?=\s+\+|\s+\d)\s+(?:\+(?<operation_income_value_rub>\d[\d\s]*,\d{2})|(?<operation_expense_value_rub>\d[\d\s]*,\d{2}))\s+(?<operation_transfer_date>\d{2}\.\d{2}\.\d{4})\s+(?<operation_auth_code>\d{6})\s+(?<operation_description>[\-\u2013\u2014]|[\s\S]+?\*{4}\d+)";

    public PaymentTransactionsExtractor000001()
        : base(Pattern) { }

    public override IEnumerable<PaymentTransactionMatch000001> Extract(string rawText) =>
        TransactionRegex.Matches(rawText).Select(PaymentTransactionMatch000001.FromMatch);
}
