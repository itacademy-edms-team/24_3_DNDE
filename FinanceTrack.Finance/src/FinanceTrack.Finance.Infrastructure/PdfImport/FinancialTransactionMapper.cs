using FinanceTrack.Finance.Core.FinancialTransactionAggregate;
using FinanceTrack.Finance.Infrastructure.Data.Config;
using FinanceTrack.Finance.Infrastructure.PdfImport.Abstractions;
using FinanceTrack.Finance.UseCases.ImportTransactions;

namespace FinanceTrack.Finance.Infrastructure.PdfImport;

internal static class FinancialTransactionMapper
{
    private static string TruncateWithEllipsis(string value, int maxLength)
    {
        value = value.Trim();
        if (value.Length <= maxLength)
            return value;

        const string ellipsis = "...";
        var take = Math.Max(0, maxLength - ellipsis.Length);
        return value.Substring(0, take) + ellipsis;
    }

    public static TransactionImportDto ToDto(TransactionMatchBase match)
    {
        var hasIncome = !string.IsNullOrWhiteSpace(match.IncomeAmount);
        var hasExpense = !string.IsNullOrWhiteSpace(match.ExpenseAmount);

        decimal incomeAmount = 0m;
        decimal expenseAmount = 0m;

        if (hasIncome)
            incomeAmount = PdfValueParser.ParseAmount(match.IncomeAmount);

        if (hasExpense)
            expenseAmount = PdfValueParser.ParseAmount(match.ExpenseAmount);

        var isIncome = incomeAmount >= 0.01m;

        var fullName = match.Description.Trim();
        var nameMaxLength = FinancialTransactionDataSchemaConstants.TransactionNameMaxLength;
        var descriptionMaxLength =
            FinancialTransactionDataSchemaConstants.TransactionDescriptionMaxLength;

        string? description = null;
        var name = fullName.Trim();

        if (fullName.Length > nameMaxLength)
        {
            name = TruncateWithEllipsis(fullName, nameMaxLength);
            description = TruncateWithEllipsis(fullName, descriptionMaxLength);
        }

        return new TransactionImportDto(
            Name: name,
            Description: description,
            TransactionType: isIncome
                ? FinancialTransactionType.Income
                : FinancialTransactionType.Expense,
            Amount: isIncome ? incomeAmount : expenseAmount,
            OperationDate: PdfValueParser.ParseDate(match.OperationDate)
        );
    }
}
