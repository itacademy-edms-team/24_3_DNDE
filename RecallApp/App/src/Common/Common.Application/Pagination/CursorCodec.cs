using System.Text.Json;

namespace Common.Application.Pagination;

/// <summary>
/// Кодировщик курсора в base64 строчку.
/// </summary>
public static class CursorCodec
{
    public static string Encode<TPayload>(TPayload payload)
        where TPayload : notnull =>
        ToBase64Url(JsonSerializer.SerializeToUtf8Bytes(payload));

    /// <summary>
    /// Пробует разобрать курсор. false - строка битая или не того формата.
    /// </summary>
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
