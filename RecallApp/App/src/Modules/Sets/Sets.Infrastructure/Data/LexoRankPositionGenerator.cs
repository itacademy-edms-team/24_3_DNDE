using AlgoLexoRank = LexoRank.LexoRank;
using Sets.Domain.Elements;

namespace Sets.Infrastructure.Data;

/// <summary>
/// Предоставляет ранги порядка на основе алгоритма LexoRank.
/// </summary>
/// <remarks>
/// Ранг - строка вида <c>0|hzzzzz:</c>, между любыми двумя значениями которой всегда есть третье.
/// Библиотека считает середину детерминированно: повтор с теми же соседями даёт тот же ранг.
/// Бакеты и перебалансировка не используются: при текущих объёмах длина строки не растёт
/// до проблемных значений.
/// </remarks>
internal sealed class LexoRankPositionGenerator : IPositionGenerator
{
    /// <summary>
    /// Возвращает ранг между соседями.
    /// </summary>
    /// <param name="previous">Ранг перед вставкой; <see langword="null"/> - начало списка.</param>
    /// <param name="next">Ранг после вставки; <see langword="null"/> - конец списка.</param>
    /// <returns>Новый ранг между указанными значениями.</returns>
    public Position Between(Position? previous, Position? next)
    {
        var rank = AlgoLexoRank.CalculateBetween(previous?.Value, next?.Value);
        return Position.FromStorage(rank.ToString());
    }
}
