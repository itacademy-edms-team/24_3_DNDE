namespace FinanceTrack.Finance.Core.UserAggregate.Specifications;

public class UserByTelegramChatIdSpec : Specification<User>, ISingleResultSpecification<User>
{
    public UserByTelegramChatIdSpec(long telegramChatId)
    {
        Query.Where(u => u.TelegramChatId == telegramChatId);
    }
}
