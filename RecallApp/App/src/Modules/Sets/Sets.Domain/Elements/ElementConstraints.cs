namespace Sets.Domain.Elements;

/// <summary>
/// Предоставляет числовые ограничения элемента набора.
/// </summary>
/// <remarks>
/// Единственный источник значений для EF-конфигурации, value objects и валидаторов (ADR 0038);
/// значения закреплены тестом-якорем.
/// </remarks>
public static class ElementConstraints
{
    /// <summary>Максимальная длина термина после trim.</summary>
    public const int TermMaxLength = 500;

    /// <summary>Максимальная длина определения после trim.</summary>
    public const int DefinitionMaxLength = 2000;
}

/// <summary>
/// Предоставляет ограничения постраничной выдачи элементов набора.
/// </summary>
/// <remarks>
/// Значения задают допустимый диапазон <c>pageSize</c> в запросе списка элементов (ADR 0039).
/// </remarks>
public static class ElementListConstraints
{
    /// <summary>
    /// Минимальный размер страницы.
    /// </summary>
    public const int MinPageSize = 1;

    /// <summary>
    /// Максимальный размер страницы.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Размер страницы по умолчанию, если клиент его не указал.
    /// </summary>
    /// <remarks>
    /// Находится в пределах нижней и верхней границ.
    /// </remarks>
    public const int DefaultPageSize = 100;
}
