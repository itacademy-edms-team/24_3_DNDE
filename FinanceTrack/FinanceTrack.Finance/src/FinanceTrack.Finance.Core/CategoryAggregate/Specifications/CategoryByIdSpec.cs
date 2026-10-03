using Ardalis.Specification;

namespace FinanceTrack.Finance.Core.CategoryAggregate.Specifications;

public class CategoryByIdSpec : Specification<Category>
{
    public CategoryByIdSpec(Guid categoryId)
    {
        Query.Where(c => c.Id == categoryId);
    }
}
