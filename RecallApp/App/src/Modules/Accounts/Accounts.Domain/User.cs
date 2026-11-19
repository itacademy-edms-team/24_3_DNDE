using ErrorOr;

namespace Accounts.Domain;

/// <summary>
/// Представляет доменного пользователя - строку, связывающую личность из Keycloak с данными приложения.
/// </summary>
/// <remarks>
/// <para>Создаётся JIT при первом доменном запросе,
/// ключ связи - <see cref="KeycloakSub"/> (ADR 0020).</para>
/// <para>Email - кэш: источник истины в Keycloak, значение обновляется из claims.
/// Отображаемое имя принадлежит приложению.</para>
/// </remarks>
public sealed class User
{
    private User() { }

    /// <summary>Возвращает идентификатор пользователя.</summary>
    public Guid Id { get; private set; }

    /// <summary>Возвращает идентификатор пользователя в Keycloak.</summary>
    public Guid KeycloakSub { get; private set; }

    /// <summary>Возвращает кэш email для фоновых задач и уведомлений.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Возвращает отображаемое имя; источник истины - приложение.</summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>Возвращает момент создания пользователя в UTC.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Возвращает момент последнего доменного изменения в UTC.</summary>
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

        if (name.Length > UserConstraints.DisplayNameMaxLength)
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
