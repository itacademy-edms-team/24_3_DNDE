using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Sets.Application.Sets;

/// <summary>
/// Предоставляет хеш канонической формы запроса на создание набора.
/// </summary>
/// <remarks>
/// Канонизация нужна, чтобы один и тот же по смыслу запрос давал одинаковый хеш: 
/// строки обрезаются по краям, пустое описание приводится к <see langword="null"/>.
/// Без неё повтор с другим форматированием получил бы ложный конфликт ключа идемпотентности.
/// </remarks>
public static class CreateSetRequestHash
{
    /// <summary>
    /// Вычисляет хеш канонического представления тела запроса.
    /// </summary>
    /// <param name="title">Название из запроса.</param>
    /// <param name="description">Описание из запроса.</param>
    /// <returns>SHA-256 в hex-представлении как ключ сравнения тел запросов.</returns>
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
