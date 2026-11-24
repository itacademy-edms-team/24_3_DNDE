namespace Sets.Domain.Sets;

/// <summary>
/// Предоставляет ограничения постраничной выдачи списка наборов.
/// </summary>
/// <remarks>
/// Значения задают допустимый диапазон <c>pageSize</c> в запросе списка наборов (ADR 0039).
/// </remarks>
public static class SetPaginationConstraints
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
    public const int DefaultPageSize = 20;
}
