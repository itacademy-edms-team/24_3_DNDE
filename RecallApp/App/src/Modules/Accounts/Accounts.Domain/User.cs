using ErrorOr;

namespace Accounts.Domain;

/// <summary>
/// Доменный пользователь: строка, представляющая человека в системе.
/// Создаётся JIT из claims личности Keycloak (ADR 0020), ключ связи - KeycloakSub.
/// </summary>
public sealed class User
{
    public const int DisplayNameMaxLength = 100;

    private User() { }

    public Guid Id { get; private set; }

    public Guid KeycloakSub { get; private set; }

    /// <summary>Кэш email для фоновых задач и уведомлений.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Доменное отображаемое имя; источник - приложение.</summary>
    public string DisplayName { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static ErrorOr<User> Create(Guid keycloakSub, string email, string displayName, DateTime utcNow)
    {
        if (keycloakSub == Guid.Empty)
            return UserErrors.KeycloakSubInvalid;

        if (string.IsNullOrWhiteSpace(email))
            return UserErrors.EmailRequired;

        var name = displayName.Trim();
        if (name.Length == 0)
            return UserErrors.DisplayNameRequired;

        if (name.Length > DisplayNameMaxLength)
            return UserErrors.DisplayNameTooLong(name.Length);

        return new User
        {
            Id = Guid.NewGuid(),
            KeycloakSub = keycloakSub,
            Email = email.Trim().ToLowerInvariant(),
            DisplayName = name,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
        };
    }

    /// <summary>
    /// Актуализирует кэш email из claims. Не считается доменным изменением: UpdatedAt не трогает.
    /// </summary>
    public void RefreshEmailContact(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        Email = email.Trim().ToLowerInvariant();
    }
}
