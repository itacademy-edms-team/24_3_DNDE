using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FinanceTrack.Finance.Core.Shared;

namespace FinanceTrack.Finance.Core.UserAggregate;

public class User : GuidEntityBase, IAggregateRoot
{
    public string Email { get; private set; } = string.Empty;

    public bool IsEmailNotificationsEnabled { get; private set; } = false;
    public long? TelegramChatId { get; private set; }
    public bool IsTelegramNotificationsEnabled { get; private set; } = false;

    // ORM
    private User() { }

    private User(Guid id, string email)
    {
        Id = id; // Исключение в поведении. Убираем авто-генерацию id. Устанавливаем вручную.
        Email = email;
    }

    public static User Create(string keycloakId, string email)
    {
        Guard.Against.Default(keycloakId);
        Guard.Against.NullOrEmpty(email);

        return new User(Guid.Parse(keycloakId), email);
    }

    public User UpdateEmail(string email)
    {
        Guard.Against.NullOrEmpty(email);
        Email = email;
        return this;
    }

    public User EnableEmailNotifications()
    {
        IsEmailNotificationsEnabled = true;
        return this;
    }

    public User DisableEmailNotifications()
    {
        IsEmailNotificationsEnabled = false;
        return this;
    }

    public User ConnectTelegramBot(long telegramChatId)
    {
        TelegramChatId = telegramChatId;
        IsTelegramNotificationsEnabled = true;
        return this;
    }

    public User DisconnectTelegramBot()
    {
        TelegramChatId = null;
        IsTelegramNotificationsEnabled = false;
        return this;
    }
}
