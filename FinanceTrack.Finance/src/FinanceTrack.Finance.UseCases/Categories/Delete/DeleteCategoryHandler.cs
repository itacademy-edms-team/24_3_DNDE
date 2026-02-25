using FinanceTrack.Finance.Core.CategoryAggregate;

namespace FinanceTrack.Finance.UseCases.Categories.Delete;

public sealed class DeleteCategoryHandler(IRepository<Category> _repo)
    : ICommandHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category = await _repo.GetByIdAsync(request.CategoryId, ct);
        if (category is null)
            return Result.NotFound();
        if (!string.Equals(category.UserId, request.UserId, StringComparison.Ordinal))
            return Result.Forbidden();

        await _repo.DeleteAsync(category, ct);
        return Result.Success();
    }
}
