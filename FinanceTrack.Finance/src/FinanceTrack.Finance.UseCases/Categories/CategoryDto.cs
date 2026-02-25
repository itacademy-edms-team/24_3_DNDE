namespace FinanceTrack.Finance.UseCases.Categories;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Type,
    string? Icon,
    string? Color
);
