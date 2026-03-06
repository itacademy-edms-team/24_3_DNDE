namespace FinanceTrack.Finance.Core.CategoryAggregate.Specifications;

public class UserCategoriesSpec : Specification<Category>
{
    public UserCategoriesSpec(string userId)
    {
        Query
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name);
    }
}
