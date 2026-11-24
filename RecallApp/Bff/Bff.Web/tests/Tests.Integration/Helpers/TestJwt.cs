using System.Text;
using System.Text.Json;

namespace Tests.Integration.Helpers;

/// <summary>
/// Неподписанный JWT для тестов. BFF не проверяет подпись access-токена;
/// из него читается только exp, роли приходят в principal из ID-токена.
/// </summary>
public static class TestJwt
{
    /// <summary>exp по умолчанию: 2026-01-01T00:00:00Z.</summary>
    public const long DefaultExp = 1_767_225_600;

    public static string Create(long exp = DefaultExp)
    {
        var header = Encode(new { alg = "none", typ = "JWT" });
        var payload = Encode(
            new
            {
                sub = "user-1",
                // Фиксированный exp: тесты не зависят от часов.
                exp,
            }
        );
        return $"{header}.{payload}.signature";
    }

    private static string Encode(object value) =>
        Convert
            .ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value)))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
