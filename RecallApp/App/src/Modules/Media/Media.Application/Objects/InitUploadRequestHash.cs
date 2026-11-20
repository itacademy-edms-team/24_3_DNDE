using System.Security.Cryptography;
using System.Text;
using Media.Domain.Objects;

namespace Media.Application.Objects;

/// <summary>
/// Предоставляет каноническую форму запроса на инициализацию загрузки
/// для хранилища идемпотентности.
/// </summary>
/// <remarks>
/// Повтор с тем же телом должен дать тот же хеш: тип приводится к канонической форме,
/// поэтому регистр и пробелы по краям на результат не влияют.
/// </remarks>
public static class InitUploadRequestHash
{
    /// <summary>
    /// Вычисляет хеш канонической формы запроса.
    /// </summary>
    /// <param name="contentType">Заявленный тип файла.</param>
    /// <param name="sizeBytes">Заявленный размер; <see langword="null"/> -> считаем как пустую строку.</param>
    /// <returns>
    /// SHA-256 в hex-представлении как ключ сравнения тел запросов.
    /// </returns>
    public static string Compute(string? contentType, long? sizeBytes)
    {
        MediaContentTypes.TryNormalize(contentType, out var normalized);
        var canonical = string.Join('|', "media.init", normalized, sizeBytes?.ToString() ?? string.Empty);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexString(hash);
    }
}
