namespace FinanceTrack.Finance.Web.Categories;

public sealed record CategoryRecord(
    Guid Id,
    string Name,
    string Type,
    string? Icon,
    string? Color
);
