namespace FinanceTrack.Finance.UseCases.Categories.Delete;

public sealed record DeleteCategoryCommand(Guid CategoryId, string UserId) : ICommand<Result>;
