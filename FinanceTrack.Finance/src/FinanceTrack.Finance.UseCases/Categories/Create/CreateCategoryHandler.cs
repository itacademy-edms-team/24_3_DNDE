using FinanceTrack.Finance.Core.CategoryAggregate;
using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.UseCases.Categories.Create;

public sealed class CreateCategoryHandler(
    IRepository<Category> _repo,
    IUnitOfWork _unitOfWork
) : ICommandHandler<CreateCategoryCommand, Result<Guid>>
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
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success(category.Id);
    }
}
