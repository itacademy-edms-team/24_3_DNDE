using System.Text.Json;

namespace Common.Application.Pagination;

/// <summary>
/// Предоставляет кодирование курсора в base64url-строку и обратное декодирование.
/// </summary>
public static class CursorCodec
{
    /// <summary>
    /// Кодирует полезную нагрузку курсора в строку.
    /// </summary>
    /// <typeparam name="TPayload">Тип полезной нагрузки курсора.</typeparam>
    /// <param name="payload">Полезная нагрузка курсора.</param>
    /// <returns>base64url-строка без символа заполнения.</returns>
    public static string Encode<TPayload>(TPayload payload)
        where TPayload : notnull =>
        ToBase64Url(JsonSerializer.SerializeToUtf8Bytes(payload));

    /// <summary>
    /// Пробует разобрать курсор.
    /// </summary>
    /// <typeparam name="TPayload">Тип полезной нагрузки курсора.</typeparam>
    /// <param name="cursor">Строка курсора из запроса.</param>
    /// <param name="payload">Распознанная полезная нагрузка; при неудаче - значение по умолчанию.</param>
    /// <returns>
    /// <see langword="true"/>, если курсор разобран;
    /// <see langword="false"/>, если строка битая или не того формата.
    /// </returns>
    public static bool TryDecode<TPayload>(string? cursor, out TPayload? payload)
        where TPayload : class
    {
        payload = null;
        if (string.IsNullOrWhiteSpace(cursor))
            return false;

        try
        {
            payload = JsonSerializer.Deserialize<TPayload>(FromBase64Url(cursor));
            return payload is not null;
        }
        catch (Exception exception) when (exception is FormatException or JsonException or ArgumentException)
        {
            return false;
        }
    }

    private static string ToBase64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] FromBase64Url(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '='));
    }
}
