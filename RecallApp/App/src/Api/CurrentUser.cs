using Common.Application.Provisioning;

namespace Api;

/// <summary>
/// Реализует <see cref="ICurrentUser"/>: держит идентичность доменного пользователя на время запроса.
/// </summary>
/// <remarks>
/// Заполняется ступенью JIT-провижининга в начале запроса,
/// читается модулями через <see cref="ICurrentUser"/>.
/// </remarks>
internal sealed class CurrentUser : ICurrentUser
{
    /// <summary>
    /// Возвращает идентификатор доменного пользователя;
    /// <see langword="null"/>, пока провижининг его не установил.
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// Задает идентификатор доменного пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор доменного пользователя.</param>
    public void SetUserId(Guid userId) => UserId = userId;
}
