using System.Security.Cryptography;
using System.Text;

namespace Media.Application.Objects;

/// <summary>
/// Каноническая форма запроса подтверждения для журнала идемпотентности.
/// </summary>
public static class ConfirmUploadRequestHash
{
    public static string Compute(Guid mediaId)
    {
        // Операция привязана к конкретному файлу, поэтому его идентификатор должен входить в хеш.
        var canonical = string.Join('|', "media.confirm", mediaId.ToString("D"));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexString(hash);
    }
}
