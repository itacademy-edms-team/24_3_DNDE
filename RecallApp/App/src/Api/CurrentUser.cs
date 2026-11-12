using Common.Application.Provisioning;

namespace Api;

/// <summary>
/// Scoped-держатель идентичности доменного пользователя:
/// заполняется ступенью JIT-провижининга в начале запроса, читается модулями через <see cref="ICurrentUser"/>.
/// </summary>
internal sealed class CurrentUser : ICurrentUser
{
    public Guid? UserId { get; private set; }

    public void SetUserId(Guid userId) => UserId = userId;
}
