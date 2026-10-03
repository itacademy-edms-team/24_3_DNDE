namespace FinanceTrack.Finance.Core.WalletAggregate;

public class WalletType : SmartEnum<WalletType>
{
    public static readonly WalletType Checking = new(nameof(Checking), 1);
    public static readonly WalletType Savings = new(nameof(Savings), 2);

    protected WalletType(string name, int value)
        : base(name, value) { }
}
