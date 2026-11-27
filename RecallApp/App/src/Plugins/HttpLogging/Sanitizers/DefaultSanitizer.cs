using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using HttpLogging.Abstractions;
using Microsoft.Extensions.Primitives;

namespace HttpLogging.Sanitizers;

/// <summary>
/// Маскирует сенситивные значения в заголовках, query-параметрах и телах запроса/ответа.
/// </summary>
/// <remarks>
/// Заголовки (<see cref="Microsoft.AspNetCore.Http.IHeaderDictionary"/>) и query-параметры
/// (<see cref="Microsoft.AspNetCore.Http.IQueryCollection"/>) маскируются по именам ключей.
/// JSON-тела обходятся рекурсивно,
/// строки маскируются по шаблону "ключ=значение",
/// прочие значения возвращаются без изменений.
/// </remarks>
internal sealed class DefaultSanitizer : ISanitizer
{
    private readonly SanitizerOptions _options;
    private readonly Regex _textRegex;

    public DefaultSanitizer(SanitizerOptions? options = null)
    {
        _options = options ?? new SanitizerOptions();
        var escapedKeys = string.Join("|", _options.SensitiveKeys.Select(Regex.Escape));
        _textRegex = new Regex($@"({escapedKeys})=[^&\s""']+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    public object? Sanitize(object? rawData)
    {
        if (rawData is null)
            return null;

        if (rawData is IEnumerable<KeyValuePair<string, StringValues>> pairs)
        {
            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var (key, value) in pairs)
            {
                if (_options.SensitiveKeys.Contains(key))
                    dict[key] = _options.MaskValue;
                else
                    dict[key] = value.Count == 1 ? value[0] : value.ToArray();
            }

            return dict;
        }

        if (rawData is JsonNode jsonNode)
        {
            MaskJsonNode(jsonNode);
            return jsonNode;
        }

        if (rawData is string text)
        {
            return _options.SensitiveKeys.Count > 0
                ? _textRegex.Replace(text, $"$1={_options.MaskValue}")
                : text;
        }

        // Числа, даты, byte[] возвращаем как есть.
        return rawData;
    }

    private void MaskJsonNode(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var property in obj.ToList())
                {
                    if (_options.SensitiveKeys.Contains(property.Key))
                        obj[property.Key] = _options.MaskValue;
                    else
                        MaskJsonNode(property.Value);
                }
                break;

            case JsonArray array:
                foreach (var item in array)
                    MaskJsonNode(item);
                break;
        }
    }
}
