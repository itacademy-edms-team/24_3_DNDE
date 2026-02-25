using FinanceTrack.Finance.Core.CategoryAggregate;

namespace FinanceTrack.Finance.UseCases.Categories.Create;

public sealed class CreateCategoryHandler(IRepository<Category> _repo)
    : ICommandHandler<CreateCategoryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        if (!CategoryType.TryFromName(request.Type, ignoreCase: true, out var categoryType))
            return Result.Error($"Invalid category type: {request.Type}. Must be 'Income' or 'Expense'.");

        var category = Category.Create(
            request.UserId,
            request.Name,
            categoryType,
            request.Icon,
            request.Color
        );

        await _repo.AddAsync(category, ct);
        return Result.Success(category.Id);
    }
}
