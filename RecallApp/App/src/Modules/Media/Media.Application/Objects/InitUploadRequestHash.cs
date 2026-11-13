using System.Security.Cryptography;
using System.Text;
using Media.Domain.Objects;

namespace Media.Application.Objects;

/// <summary>
/// Каноническая форма запроса на инициализацию загрузки для журнала идемпотентности:
/// повтор с тем же телом должен совпасть побайтно, регистр типа и порядок полей не должны влиять.
/// </summary>
public static class InitUploadRequestHash
{
    public static string Compute(string? contentType, long? sizeBytes)
    {
        MediaContentTypes.TryNormalize(contentType, out var normalized);
        var canonical = string.Join('|', "media.init", normalized, sizeBytes?.ToString() ?? string.Empty);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexString(hash);
    }
}
