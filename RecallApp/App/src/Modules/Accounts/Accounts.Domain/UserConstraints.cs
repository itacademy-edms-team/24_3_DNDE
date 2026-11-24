namespace Accounts.Domain;

/// <summary>
/// Содержит ограничения домена пользователя.
/// </summary>
/// <remarks>
/// Единственный источник значений для валидации и конфигурации EF (ADR 0038)
/// и для других проверяющих объектов.
/// </remarks>
public static class UserConstraints
{
	/// <summary>Максимальная длина отображаемого имени после trim.</summary>
	public const int DisplayNameMaxLength = 100;
}