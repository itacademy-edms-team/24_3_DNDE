namespace Media.Domain.Objects;

/// <summary>
/// Указывает формат изображения.
/// </summary>
/// <remarks>
/// Числовые значения закреплены явно и сохраняются в базе как есть:
/// новые форматы добавляются только в конец.
/// </remarks>
public enum MediaFormat
{
    Png = 0,
    Jpeg = 1,
    Webp = 2,
}
