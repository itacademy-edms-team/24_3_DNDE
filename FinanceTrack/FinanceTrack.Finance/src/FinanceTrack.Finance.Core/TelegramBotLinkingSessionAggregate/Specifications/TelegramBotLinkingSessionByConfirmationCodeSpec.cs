namespace FinanceTrack.Finance.Core.TelegramBotLinkingSessionAggregate.Specifications;

public class TelegramBotLinkingSessionByConfirmationCodeSpec
    : Specification<TelegramBotLinkingSession>,
        ISingleResultSpecification<TelegramBotLinkingSession>
{
    public TelegramBotLinkingSessionByConfirmationCodeSpec(string userId, int confirmationCode)
    {
        Query.Where(s => s.UserId == userId && s.ConfirmationCode == confirmationCode);
    }
}
