namespace FinanceTrack.Finance.Core.UserAggregate.Specifications;

public class UserByIdSpec : Specification<User>, ISingleResultSpecification<User>
{
    public UserByIdSpec(Guid userId)
    {
        Query.Where(u => u.Id == userId);
    }
}
