using System.Text.RegularExpressions;
using FinanceTrack.Finance.Infrastructure.PdfImport.Abstractions;

namespace FinanceTrack.Finance.Infrastructure.PdfImport.Extractors.Sberbank.Payment;

public class PaymentTransactionMatch000001 : TransactionMatchBase
{
    // Common
    public override string OperationDate { get; init; } = "";
    public override string TransferDate { get; init; } = "";
    public override string IncomeAmount { get; init; } = "";
    public override string ExpenseAmount { get; init; } = "";
    public override string Description { get; init; } = "";

    // Sberbank-specific
    public string OperationTime { get; init; } = "";
    public string Category { get; init; } = "";
    public string AuthCode { get; init; } = "";

    public static PaymentTransactionMatch000001 FromMatch(Match match) =>
        new()
        {
            OperationDate = match.Groups["operation_debit_date"].Value,
            TransferDate = match.Groups["operation_transfer_date"].Value,
            IncomeAmount = match.Groups["operation_income_value_rub"].Value,
            ExpenseAmount = match.Groups["operation_expense_value_rub"].Value,
            Description = match.Groups["operation_description"].Value,
            OperationTime = match.Groups["operation_time"].Value,
            Category = match.Groups["operation_category"].Value,
            AuthCode = match.Groups["operation_auth_code"].Value,
        };
}
