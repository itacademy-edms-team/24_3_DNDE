using System.Security.Cryptography;
using System.Text;

namespace Media.Application.Objects;

/// <summary>
/// Предоставляет каноническую форму запроса подтверждения для хранилища идемпотентности.
/// </summary>
/// <remarks>
/// Операция привязана к конкретному файлу, поэтому идентификатор файла входит в хеш.
/// </remarks>
public static class ConfirmUploadRequestHash
{
    /// <summary>
    /// Вычисляет хеш канонической формы запроса.
    /// </summary>
    /// <param name="mediaId">Файл, подтверждение которого хешируется.</param>
    /// <returns>Хеш SHA-256 канонической формы в шестнадцатеричном виде.</returns>
    public static string Compute(Guid mediaId)
    {
        var canonical = string.Join('|', "media.confirm", mediaId.ToString("D"));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexString(hash);
    }
}
