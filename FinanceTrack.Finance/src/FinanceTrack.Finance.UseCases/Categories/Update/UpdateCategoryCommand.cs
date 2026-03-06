namespace FinanceTrack.Finance.UseCases.Categories.Update;

public sealed record UpdateCategoryCommand(
    Guid CategoryId,
    string UserId,
    string Name,
    string? Icon,
    string? Color
) : ICommand<Result<CategoryDto>>;
