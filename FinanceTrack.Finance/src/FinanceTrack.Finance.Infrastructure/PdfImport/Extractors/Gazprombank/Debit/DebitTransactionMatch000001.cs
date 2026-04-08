using System.Text.RegularExpressions;
using FinanceTrack.Finance.Infrastructure.PdfImport.Abstractions;

namespace FinanceTrack.Finance.Infrastructure.PdfImport.Extractors.Gazprombank.Debit;

public class DebitTransactionMatch000001 : TransactionMatchBase
{
    public override string OperationDate { get; init; } = "";
    public override string TransferDate { get; init; } = "";
    public override string IncomeAmount { get; init; } = "";
    public override string ExpenseAmount { get; init; } = "";
    public override string Description { get; init; } = "";

    public static DebitTransactionMatch000001 FromMatch(Match match) =>
        new()
        {
            OperationDate = match.Groups["operation_debit_date"].Value,
            TransferDate = match.Groups["operation_transfer_date"].Value,
            IncomeAmount = match.Groups["operation_income_value_rub"].Value,
            ExpenseAmount = match.Groups["operation_expense_value_rub"].Value,
            Description = match.Groups["operation_description"].Value,
        };
}
