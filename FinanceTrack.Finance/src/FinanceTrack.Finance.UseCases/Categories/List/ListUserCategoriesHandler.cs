using FinanceTrack.Finance.Core.CategoryAggregate;
using FinanceTrack.Finance.Core.CategoryAggregate.Specifications;

namespace FinanceTrack.Finance.UseCases.Categories.List;

public sealed class ListUserCategoriesHandler(IReadRepository<Category> _repo)
    : IQueryHandler<ListUserCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public async Task<IReadOnlyList<CategoryDto>> Handle(
        ListUserCategoriesQuery request,
        CancellationToken ct
    )
    {
        var spec = new UserCategoriesSpec(request.UserId);
        var categories = await _repo.ListAsync(spec, ct);

        return categories
            .Select(c => new CategoryDto(c.Id, c.Name, c.Type.Name, c.Icon, c.Color))
            .ToList();
    }
}
