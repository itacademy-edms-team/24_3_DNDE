namespace FinanceTrack.Finance.UseCases.Categories.Create;

public sealed record CreateCategoryCommand(
    string UserId,
    string Name,
    string Type, // "Income" or "Expense"
    string? Icon,
    string? Color
) : ICommand<Result<Guid>>;
