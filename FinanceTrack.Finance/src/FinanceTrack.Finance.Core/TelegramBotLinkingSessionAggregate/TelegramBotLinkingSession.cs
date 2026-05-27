using FinanceTrack.Finance.Core.Shared;

namespace FinanceTrack.Finance.Core.TelegramBotLinkingSessionAggregate;

public sealed class TelegramBotLinkingSession : GuidEntityBase, IAggregateRoot
{
    public string UserId { get; private set; } = default!;
    public Guid PrimaryCode { get; private set; }
    public int? ConfirmationCode { get; private set; }
    public long? TelegramChatId { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAtUtc;
    public bool HasConfirmation => ConfirmationCode is not null;

    // ORM
    private TelegramBotLinkingSession() { }

    public static TelegramBotLinkingSession Create(
        string userId,
        Guid primaryCode,
        DateTime expiresAt
    )
    {
        Guard.Against.NullOrEmpty(userId);
        Guard.Against.Default(primaryCode);

        return new TelegramBotLinkingSession
        {
            UserId = userId,
            PrimaryCode = primaryCode,
            ExpiresAtUtc = expiresAt,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public void SetConfirmation(int confirmationCode, long telegramChatId)
    {
        ConfirmationCode = confirmationCode;
        TelegramChatId = telegramChatId;
    }
}
