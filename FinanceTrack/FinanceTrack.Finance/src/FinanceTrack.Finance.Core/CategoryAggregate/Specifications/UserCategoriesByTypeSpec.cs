namespace FinanceTrack.Finance.Core.CategoryAggregate.Specifications;

public class UserCategoriesByTypeSpec : Specification<Category>
{
    public UserCategoriesByTypeSpec(string userId, CategoryType type)
    {
        Query
            .Where(c => c.UserId == userId && c.Type == type)
            .OrderBy(c => c.Name);
    }
}
