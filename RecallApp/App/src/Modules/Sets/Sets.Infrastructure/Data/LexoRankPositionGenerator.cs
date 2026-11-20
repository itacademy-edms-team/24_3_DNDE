using AlgoLexoRank = LexoRank.LexoRank;
using Sets.Domain.Elements;

namespace Sets.Infrastructure.Data;

/// <summary>
/// Предоставляет ранги порядка на основе алгоритма LexoRank.
/// </summary>
/// <remarks>
/// <para>Позиция - это строка вида <c>0|hzzzzz:</c>.
/// За счёт особенностей сортировки строк между двух строк всегда можно вставить третью.</para>
/// <para>Середина вычисляется детерминированно: повтор с теми же соседями даёт тот же ранг.</para>
/// <para>Бакеты и перебалансировка не используются: длина ранга растёт медленно, и при текущих
/// объёмах не доходит до значений, где нужна перебалансировка.</para>
/// <para>Корректность границ обеспечивает вызывающий: сюда пара с нарушенным порядком не попадает.
/// Библиотека бросает исключение, если между границами нет места или правая граница - минимальный ранг;
/// такие значения генератор не выдаёт.</para>
/// </remarks>
internal sealed class LexoRankPositionGenerator : IPositionGenerator
{
    /// <summary>
    /// Возвращает ранг между соседями.
    /// </summary>
    /// <param name="previous">Сосед слева; <see langword="null"/> - начало набора.</param>
    /// <param name="next">Сосед справа; <see langword="null"/> - конец набора.</param>
    /// <returns>Ранг, встающий после <paramref name="previous"/> и перед <paramref name="next"/>.</returns>
    public Position Between(Position? previous, Position? next)
    {
        var rank = AlgoLexoRank.CalculateBetween(previous?.Value, next?.Value);
        return Position.FromStorage(rank.ToString());
    }
}
