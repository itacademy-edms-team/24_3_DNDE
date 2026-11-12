namespace Sets.Domain.Elements;

/// <summary>
/// Числовые ограничения элемента набора. Единственный источник значений для EF-конфигурации,
/// value objects и валидаторов (ADR 0038); значения закреплены тестом-якорем.
/// </summary>
public static class ElementConstraints
{
    /// <summary>Максимальная длина термина после trim.</summary>
    public const int TermMaxLength = 500;

    /// <summary>Максимальная длина определения после trim.</summary>
    public const int DefinitionMaxLength = 2000;
}

/// <summary>Границы постраничной выдачи элементов набора (ADR 0039).</summary>
public static class ElementListConstraints
{
    public const int MinPageSize = 1;

    /// <summary>Дефолт совпадает с верхней границей: редактор грузит набор целиком за один запрос.</summary>
    public const int MaxPageSize = 100;

    public const int DefaultPageSize = 100;
}
