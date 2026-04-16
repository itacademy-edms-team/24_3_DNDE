using Ardalis.SmartEnum;

namespace FinanceTrack.Finance.UseCases.ImportTransactions;

public class BankStatementType : SmartEnum<BankStatementType>
{
    public static readonly BankStatementType SberbankPayment000001 = new(
        nameof(SberbankPayment000001),
        1
    );
    public static readonly BankStatementType GazprombankDebit000001 = new(
        nameof(GazprombankDebit000001),
        2
    );

    private BankStatementType(string name, int value)
        : base(name, value) { }
}
