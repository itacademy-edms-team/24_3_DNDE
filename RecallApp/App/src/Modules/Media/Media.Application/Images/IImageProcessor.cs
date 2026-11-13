using ErrorOr;
using Media.Domain.Objects;

namespace Media.Application.Images;

/// <summary>Готовый производный вариант объекта: байты WebP и фактические размеры.</summary>
public sealed record ProcessedVariant(MediaVariantName Variant, byte[] Content, int Width, int Height);

/// <summary>Результат обработки загруженного изображения: размеры исходника и два варианта.</summary>
public sealed record ProcessedImage(
    MediaFormat Format,
    int Width,
    int Height,
    IReadOnlyList<ProcessedVariant> Variants);

/// <summary>
/// Обработчик загруженного изображения: проверка сигнатуры и наличия анимации, лимиты до декодирования,
/// перекодирование в WebP со снятием метаданных и нарезка вариантов (ADR 0018, ADR 0037).
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// Проверяет и перекодирует изображение.
    /// Ошибки - доменные коды Media.*: неподдерживаемый формат, анимация, превышение лимитов, повреждённый файл.
    /// </summary>
    Task<ErrorOr<ProcessedImage>> ProcessAsync(byte[] source, CancellationToken cancellationToken);
}
