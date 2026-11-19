namespace Media.Domain.Objects;

/// <summary>
/// Указывает вид производного варианта изображения.
/// </summary>
/// <remarks>
/// <para>Список закрыт (онтология Media §5): приложение рассчитывает ровно на два варианта,
/// и <see cref="MediaObject.MarkReady"/> принимает только полный набор.</para>
/// <para>Добавление нового размера - это новое значение, миграция данных и фоновая
/// переобработка существующих файлов, а не только новая запись в перечислении.</para>
/// <para>Числовые значения закреплены явно и сохраняются в базе как есть.</para>
/// </remarks>
public enum MediaVariantName
{
    Thumb = 0,
    Display = 1,
}

/// <summary>
/// Предоставляет строковые имена вариантов.
/// </summary>
/// <remarks>
/// Имена попадают в ключи объектного хранилища и в ответы API, поэтому переименование значения
/// ломает доступ к уже загруженным файлам: имя меняется только вместе с миграцией ключей.
/// </remarks>
public static class MediaVariantNames
{
    /// <summary>Строковое имя варианта-миниатюры.</summary>
    public const string Thumb = "thumb";
    
    /// <summary>Строковое имя увеличенного варианта.</summary>
    public const string Display = "display";

    /// <summary>
    /// Возвращает строковое имя варианта для ключа хранилища и ответа API.
    /// </summary>
    /// <param name="variant">Вид варианта.</param>
    /// <returns>
    /// Имя варианта, совпадающее с одним из значений <see cref="MediaVariantName"/>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Значение не определено в <see cref="MediaVariantName"/>.
    /// </exception>
    public static string ToStorageName(this MediaVariantName variant) => variant switch
    {
        MediaVariantName.Thumb => Thumb,
        MediaVariantName.Display => Display,
        _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, "Unknown image variant."),
    };

    /// <summary>
    /// Пробует разобрать строковое имя варианта.
    /// </summary>
    /// <param name="value">Строковое имя из ключа хранилища или запроса.</param>
    /// <param name="variant">Распознанный вариант; при неудаче - значение по умолчанию.</param>
    /// <returns>
    /// <see langword="true"/>, если имя распознано; иначе <see langword="false"/>.
    /// </returns>
    public static bool TryParse(string? value, out MediaVariantName variant)
    {
        switch (value)
        {
            case Thumb:
                variant = MediaVariantName.Thumb;
                return true;
            case Display:
                variant = MediaVariantName.Display;
                return true;
            default:
                variant = default;
                return false;
        }
    }
}
