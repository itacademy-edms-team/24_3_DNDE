using FinanceTrack.Finance.Core.CategoryAggregate;

namespace FinanceTrack.Finance.UseCases.Categories.Update;

public sealed class UpdateCategoryHandler(IRepository<Category> _repo)
    : ICommandHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = await _repo.GetByIdAsync(request.CategoryId, ct);
        if (category is null)
            return Result.NotFound();
        if (!string.Equals(category.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        category.UpdateName(request.Name).UpdateIcon(request.Icon).UpdateColor(request.Color);

        await _repo.UpdateAsync(category, ct);

        return Result.Success(
            new CategoryDto(
                category.Id,
                category.Name,
                category.Type.Name,
                category.Icon,
                category.Color
            )
        );
    }
}
