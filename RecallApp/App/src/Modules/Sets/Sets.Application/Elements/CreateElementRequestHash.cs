using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Sets.Domain.Elements;

namespace Sets.Application.Elements;

/// <summary>
/// Хеш канонической формы запроса на создание элемента.
/// Считается по разобранному payload: сырой JSON сохраняет порядок полей и пробелы клиента,
/// поэтому повтор с тем же смыслом дал бы другой хеш и ложный 409.
/// Позиция хешируется как пришла в запросе (null - «в конец»), а не как вычислилась под локом.
/// </summary>
public static class CreateElementRequestHash
{
    public static string Compute(ElementType type, ElementPayload payload, int? position)
    {
        var canonical = string.Join('|',
            type.ToContractValue(),
            position?.ToString(CultureInfo.InvariantCulture) ?? "null",
            ElementPayloadJson.ToStorageJson(payload));

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }
}
