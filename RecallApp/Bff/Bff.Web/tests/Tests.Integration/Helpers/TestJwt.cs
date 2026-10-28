using System.Text;
using System.Text.Json;

namespace Tests.Integration.Helpers;

/// <summary>
/// Неподписанный JWT для тестов. BFF не проверяет подпись access-токена,
/// /session только читает из него client-роли RecallApp (resource_access).
/// </summary>
public static class TestJwt
{
    public static string Create(string role)
    {
        var header = Encode(new { alg = "none", typ = "JWT" });
        var payload = Encode(
            new { sub = "user-1", resource_access = new { RecallApp = new { roles = new[] { role } } } }
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
