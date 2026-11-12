using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Sets.Application.Sets;

/// <summary>
/// Хеш канонической формы запроса на создание набора.
/// Канонизация нужна, чтобы один и тот же по смыслу запрос давал одинаковый хеш:
/// строки тримятся, пустое описание нормализуется в null.
/// </summary>
public static class CreateSetRequestHash
{
    public static string Compute(string? title, string? description)
    {
        var canonical = JsonSerializer.Serialize(new
        {
            title = title?.Trim(),
            description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
        });

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }
}
