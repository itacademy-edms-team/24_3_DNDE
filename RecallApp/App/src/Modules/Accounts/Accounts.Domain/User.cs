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

    /// <summary>
    /// Создаёт пользователя из данных личности.
    /// </summary>
    /// <param name="keycloakSub">Идентификатор пользователя в Keycloak.</param>
    /// <param name="email">Email из claims.</param>
    /// <param name="displayName">Отображаемое имя; нормализуется и проверяется.</param>
    /// <param name="utcNow">Текущий момент в UTC; передаёт вызывающий.</param>
    /// <returns>Новый пользователь либо ошибка:
    /// <see cref="UserErrors.KeycloakSubInvalid"/>, <see cref="UserErrors.EmailRequired"/>,
    /// <see cref="UserErrors.DisplayNameRequired"/> или <see cref="UserErrors.DisplayNameTooLong"/>.</returns>
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
    /// Обновляет кэш email из claims.
    /// </summary>
    /// <remarks>
    /// Не считается доменным изменением: <see cref="UpdatedAt"/> не трогается,
    /// потому что источник истины - Keycloak, а не приложение.
    /// </remarks>
    /// <param name="email">Email из claims; приводится к нижнему регистру.</param>
    /// <exception cref="ArgumentException">Значение пустое или состоит из пробелов.</exception>
    public void RefreshEmailContact(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        Email = email.Trim().ToLowerInvariant();
    }
}
