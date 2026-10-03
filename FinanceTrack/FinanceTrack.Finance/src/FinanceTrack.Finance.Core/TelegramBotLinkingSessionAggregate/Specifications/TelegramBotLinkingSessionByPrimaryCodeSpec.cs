namespace FinanceTrack.Finance.Core.TelegramBotLinkingSessionAggregate.Specifications;

public class TelegramBotLinkingSessionByPrimaryCodeSpec
    : Specification<TelegramBotLinkingSession>,
        ISingleResultSpecification<TelegramBotLinkingSession>
{
    public TelegramBotLinkingSessionByPrimaryCodeSpec(Guid primaryCode)
    {
        Query.Where(s => s.PrimaryCode == primaryCode);
    }
}
