using System.Text.Json;
using System.Text.Json.Nodes;

namespace HttpLogging.Formatters;

/// <summary>
/// Предоставляет вспомогательные методы строкового представления замаскированных данных.
/// </summary>
internal static class SanitizedValue
{
    /// <summary>
    /// Преобразует замаскированные данные в строковое представление.
    /// </summary>
    /// <param name="value">Замаскированные данные.</param>
    /// <returns>
    /// Строковое представление
    /// или <see langword="null"/>.
    /// </returns>
    public static string? ToDisplayString(object? value) => value switch
    {
        null => null,
        string s => s,
        JsonNode node => node.ToJsonString(),
        _ => JsonSerializer.Serialize(value),
    };
}
