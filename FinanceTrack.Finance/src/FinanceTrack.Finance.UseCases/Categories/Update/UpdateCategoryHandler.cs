using FinanceTrack.Finance.Core.CategoryAggregate;
using FinanceTrack.Finance.Core.CategoryAggregate.Specifications;
using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.UseCases.Categories.Update;

public sealed class UpdateCategoryHandler(IRepository<Category> repo, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(
        UpdateCategoryCommand request,
        CancellationToken ct
    )
    {
        var spec = new CategoryByIdSpec(request.CategoryId);
        var category = await repo.FirstOrDefaultAsync(spec, ct);
        if (category is null)
            return Result.NotFound();
        if (!string.Equals(category.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        category.UpdateName(request.Name).UpdateIcon(request.Icon).UpdateColor(request.Color);

        await unitOfWork.SaveChangesAsync(ct);

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
