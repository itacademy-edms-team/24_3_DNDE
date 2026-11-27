using System.IO;
using System.IO.Compression;
using System.Text;
using HttpLogging.Abstractions;

namespace HttpLogging.Formatters;

/// <summary>
/// Представляет форматтер,
/// сжимающий строковое представление данных в GZip и кодирующий результат в Base64.
/// </summary>
internal sealed class GzipBase64Formatter : ILogFormatter
{
    /// <summary>
    /// Форматирует данные, сжимая их в GZip и кодируя результат в Base64.
    /// </summary>
    /// <param name="sanitizedData">Замаскированные данные.</param>
    /// <returns>
    /// Строка в кодировке Base64
    /// или <see langword="null"/>.
    /// </returns>
    public string? Format(object? sanitizedData)
    {
        var text = SanitizedValue.ToDisplayString(sanitizedData);
        if (string.IsNullOrEmpty(text))
            return null;

        var bytes = Encoding.UTF8.GetBytes(text);
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
            gzip.Write(bytes, 0, bytes.Length);

        return Convert.ToBase64String(output.ToArray());
    }
}
