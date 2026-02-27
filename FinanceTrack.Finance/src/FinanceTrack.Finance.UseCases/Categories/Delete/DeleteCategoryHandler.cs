using FinanceTrack.Finance.Core.CategoryAggregate;
using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.UseCases.Categories.Delete;

public sealed class DeleteCategoryHandler(
    IRepository<Category> _repo,
    IUnitOfWork _unitOfWork
) : ICommandHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category = await _repo.GetByIdAsync(request.CategoryId, ct);
        if (category is null)
            return Result.NotFound();
        if (!string.Equals(category.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        await _repo.DeleteAsync(category, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
