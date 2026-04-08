using Ardalis.Result;
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

    public static Result<TransactionImportDto> ToDto(TransactionMatchBase match)
    {
        var hasIncome = !string.IsNullOrWhiteSpace(match.IncomeAmount);
        var hasExpense = !string.IsNullOrWhiteSpace(match.ExpenseAmount);

        decimal incomeAmount = 0m;
        decimal expenseAmount = 0m;

        if (hasIncome && !PdfValueParser.TryParseAmount(match.IncomeAmount, out incomeAmount))
            return Result<TransactionImportDto>.Error(
                $"Invalid income amount format: '{match.IncomeAmount}'."
            );

        if (hasExpense && !PdfValueParser.TryParseAmount(match.ExpenseAmount, out expenseAmount))
            return Result<TransactionImportDto>.Error(
                $"Invalid expense amount format: '{match.ExpenseAmount}'."
            );

        var isIncome = incomeAmount >= 0.01m;
        var amount = isIncome ? incomeAmount : expenseAmount;
        if (amount < 0.01m)
            return Result<TransactionImportDto>.Error(
                "Invalid transaction amount. Parsed amount must be at least 0.01."
            );

        if (!PdfValueParser.TryParseDate(match.OperationDate, out var operationDate))
            return Result<TransactionImportDto>.Error(
                $"Invalid operation date format: '{match.OperationDate}'. Expected format: dd.MM.yyyy."
            );

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

        return Result.Success(
            new TransactionImportDto(
                Name: name,
                Description: description,
                TransactionType: isIncome
                    ? FinancialTransactionType.Income
                    : FinancialTransactionType.Expense,
                Amount: amount,
                OperationDate: operationDate
            )
        );
    }
}
