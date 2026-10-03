using FinanceTrack.Finance.Core.CategoryAggregate;
using FinanceTrack.Finance.Core.CategoryAggregate.Specifications;
using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.UseCases.Categories.Delete;

public sealed class DeleteCategoryHandler(IRepository<Category> repo, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var spec = new CategoryByIdSpec(request.CategoryId);
        var category = await repo.FirstOrDefaultAsync(spec, ct);
        if (category is null)
            return Result.NotFound();
        if (!string.Equals(category.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        await repo.DeleteAsync(category, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
