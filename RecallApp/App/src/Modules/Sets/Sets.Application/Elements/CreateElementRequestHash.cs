using System.Security.Cryptography;
using System.Text;
using Sets.Domain.Elements;

namespace Sets.Application.Elements;

/// <summary>
/// Предоставляет хеш канонической формы запроса на создание элемента.
/// </summary>
/// <remarks>
/// Хеш считается по разобранному payload, а не по сырому JSON.
/// Сырой JSON сохраняет порядок полей и пробелы клиента,
/// из-за чего повтор с тем же смыслом дал бы другой хеш и ложный конфликт ключа идемпотентности.
/// </remarks>
public static class CreateElementRequestHash
{
    /// <summary>
    /// Вычисляет хеш канонического представления команды.
    /// </summary>
    /// <param name="type">Тип элемента.</param>
    /// <param name="payload">Разобранное содержимое; берётся его стабильное JSON-представление.</param>
    /// <returns>SHA-256 в hex-представлении - ключ сравнения тел запросов.</returns>
    public static string Compute(ElementType type, ElementPayload payload)
    {
        var canonical = string.Join('|',
            type.ToContractValue(),
            ElementPayloadJson.ToStorageJson(payload));

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }
}
