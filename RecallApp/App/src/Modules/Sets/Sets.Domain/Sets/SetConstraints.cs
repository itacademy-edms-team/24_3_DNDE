namespace Sets.Domain.Sets;

/// <summary>
/// Предоставляет ограничения домена набора <see cref="Set"/>.
/// </summary>
/// <remarks>
/// Другие объекты (VO, EF Configuration, Validators, ...)
/// должны использовать этот класс как единственный источник истины.
/// Для каждого такого ограничения есть архитектурный тест, который проверяет
/// заданные по онтологии значения с приведёнными здесь.
/// </remarks>
public static class SetConstraints
{
    /// <summary>Максимальная длина названия набора после trim.</summary>
    public const int TitleMaxLength = 120;

    /// <summary>Максимальная длина описания набора после trim.</summary>
    public const int DescriptionMaxLength = 2000;
}
