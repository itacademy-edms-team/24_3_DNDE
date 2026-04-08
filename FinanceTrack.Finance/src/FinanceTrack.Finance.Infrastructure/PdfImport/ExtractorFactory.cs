using FinanceTrack.Finance.Infrastructure.PdfImport.Abstractions;
using FinanceTrack.Finance.Infrastructure.PdfImport.Extractors.Gazprombank.Debit;
using FinanceTrack.Finance.Infrastructure.PdfImport.Extractors.Sberbank.Payment;
using FinanceTrack.Finance.UseCases.ImportTransactions;

namespace FinanceTrack.Finance.Infrastructure.PdfImport;

internal static class ExtractorFactory
{
    private static readonly Dictionary<
        BankStatementType,
        ITransactionsExtractor<TransactionMatchBase>
    > _extractors = new()
    {
        [BankStatementType.SberbankPayment000001] = new PaymentTransactionsExtractor000001(),
        [BankStatementType.GazprombankDebit000001] = new DebitTransactionsExtractor000001(),
    };

    public static ITransactionsExtractor<TransactionMatchBase> GetExtractor(
        BankStatementType type
    ) =>
        _extractors.TryGetValue(type, out var extractor)
            ? extractor
            : throw new NotSupportedException($"No extractor registered for {type.Name}");
}
