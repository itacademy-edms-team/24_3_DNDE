namespace Sets.Domain.Sets;

/// <summary>
/// Числовые ограничения набора. Единственный источник значений: на них ссылаются
/// EF-конфигурация, value objects и валидаторы (ADR 0038), значения закреплены тестом-якорем.
/// </summary>
public static class SetConstraints
{
    /// <summary>Максимальная длина названия набора после trim.</summary>
    public const int TitleMaxLength = 120;

    /// <summary>Максимальная длина описания набора после trim.</summary>
    public const int DescriptionMaxLength = 2000;
}
