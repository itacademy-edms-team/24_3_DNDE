namespace Sets.Domain.Elements;

/// <summary>
/// Определяет контракт вычисления рангов порядка элементов набора.
/// </summary>
public interface IPositionGenerator
{
    /// <summary>
    /// Возвращает ранг между соседями.
    /// </summary>
    /// <param name="previous">Ранг элемента перед вставляемым; <see langword="null"/> - граница начала.</param>
    /// <param name="next">Ранг элемента после вставляемого; <see langword="null"/> - граница конца.</param>
    /// <returns>Новый ранг, лежащий между указанными значениями.</returns>
    Position Between(Position? previous, Position? next);
}
