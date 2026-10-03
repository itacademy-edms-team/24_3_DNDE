namespace FinanceTrack.Finance.Core.TelegramBotLinkingSessionAggregate.Specifications;

public class TelegramBotLinkingSessionByUserIdSpec
    : Specification<TelegramBotLinkingSession>,
        ISingleResultSpecification<TelegramBotLinkingSession>
{
    public TelegramBotLinkingSessionByUserIdSpec(string userId)
    {
        Query.Where(s => s.UserId == userId);
    }
}
