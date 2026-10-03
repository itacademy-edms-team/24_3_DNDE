namespace FinanceTrack.Finance.Core.Interfaces;

public record AiCategorySuggestion(Guid Id, string Name, string? Icon);

public interface ICategoryAiService
{
    bool IsEnabled { get; }

    Task<AiCategorySuggestion?> SuggestCategoryAsync(
        string name,
        string? description,
        string transactionType,
        string userId,
        CancellationToken cancel
    );
}
