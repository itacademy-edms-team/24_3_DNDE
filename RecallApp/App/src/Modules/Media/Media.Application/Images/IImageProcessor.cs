using ErrorOr;
using Media.Domain.Objects;

namespace Media.Application.Images;

/// <summary>
/// Представляет готовый производный вариант объекта.
/// </summary>
/// <param name="Variant">Вид варианта.</param>
/// <param name="Content">Байты WebP.</param>
/// <param name="Width">Фактическая ширина в пикселях.</param>
/// <param name="Height">Фактическая высота в пикселях.</param>
public sealed record ProcessedVariant(MediaVariantName Variant, byte[] Content, int Width, int Height);

/// <summary>
/// Представляет результат обработки загруженного изображения: размеры исходника и варианты.
/// </summary>
/// <param name="Format">Формат, определённый по сигнатуре.</param>
/// <param name="Width">Ширина исходника в пикселях.</param>
/// <param name="Height">Высота исходника в пикселях.</param>
/// <param name="Variants">Варианты, нарезанные из исходника.</param>
public sealed record ProcessedImage(
    MediaFormat Format,
    int Width,
    int Height,
    IReadOnlyList<ProcessedVariant> Variants);

/// <summary>
/// Определяет обработчик загруженного изображения.
/// </summary>
/// <remarks>
/// Проверяет сигнатуру и наличие анимации, контролирует лимиты до декодирования, перекодирует
/// в WebP со снятием метаданных и нарезает варианты (ADR 0018, ADR 0037).
/// </remarks>
public interface IImageProcessor
{
    /// <summary>
    /// Проверяет и перекодирует изображение.
    /// </summary>
    /// <param name="source">Байты загруженного файла.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Размеры исходника и варианты либо ошибка одного из кодов
    /// <see cref="MediaErrors.InvalidFormat"/>, <see cref="MediaErrors.UnsupportedAnimation"/>,
    /// <see cref="MediaErrors.FileTooLarge"/>, <see cref="MediaErrors.ResolutionTooLarge"/> или
    /// <see cref="MediaErrors.ImageCorrupt"/>.</returns>
    Task<ErrorOr<ProcessedImage>> ProcessAsync(byte[] source, CancellationToken cancellationToken);
}
