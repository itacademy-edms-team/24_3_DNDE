using System.Text.Json;
using System.Text.Json.Nodes;

namespace HttpLogging.Extractors;

/// <summary>
/// Разбирает тело http-запроса.
/// </summary>
internal static class HttpBodyParser
{
	/// <summary>
	/// Разбирает тело запроса как JSON (для рекурсивного маскирования).
	/// </summary>
	/// <param name="raw">Тело запроса</param>
	/// <returns>
	/// <see cref="JsonNode"/> в случае успешного разбора,
	/// иначе <paramref name="raw"/> или <see langword="null"/> как есть.
	/// </returns>
	public static object? TryParseJson(string? raw)
	{
		if (string.IsNullOrEmpty(raw))
			return null;

		try
		{
			return JsonNode.Parse(raw);
		}
		catch (JsonException)
		{
			return raw;
		}
	}
}