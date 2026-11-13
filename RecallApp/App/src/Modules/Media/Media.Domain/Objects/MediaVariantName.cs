namespace Media.Domain.Objects;

/// <summary>
/// Производный вариант изображения. Список закрыт (онтология Media §5): добавление нового размера -
/// новое значение плюс фоновая переобработка существующих файлов.
/// </summary>
public enum MediaVariantName
{
    Thumb = 0,
    Display = 1,
}

/// <summary>Строковые имена вариантов: они попадают в ключи хранилища и в ответ API.</summary>
public static class MediaVariantNames
{
    public const string Thumb = "thumb";
    public const string Display = "display";

    public static string ToStorageName(this MediaVariantName variant) => variant switch
    {
        MediaVariantName.Thumb => Thumb,
        MediaVariantName.Display => Display,
        _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, "Неизвестный вариант изображения."),
    };

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
