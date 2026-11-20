namespace Sets.Domain.Elements;

/// <summary>
/// Определяет генератор позиций для порядка элементов набора.
/// </summary>
/// <remarks>
/// <para>Генератор создаёт ранг, который встаёт между двумя существующими позициями;
/// <see langword="null"/> означает край набора слева или справа.</para>
/// <para>Корректность пары (previous левее next) проверяет вызывающий код: сюда некорректные
/// границы не попадают.</para>
/// <para>При одинаковых соседях генератор всегда возвращает одинаковый ранг, поэтому повторная
/// перестановка при тех же соседях не сдвигает элемент второй раз.</para>
/// <para>Реализация живёт в инфраструктуре; домен не знает, каким алгоритмом считается ранг.</para>
/// </remarks>
public interface IPositionGenerator
{
    /// <summary>
    /// Возвращает позицию между указанными соседями.
    /// </summary>
    /// <param name="previous">Сосед слева; <see langword="null"/> - начало набора.</param>
    /// <param name="next">Сосед справа; <see langword="null"/> - конец набора.</param>
    /// <returns>Ранг, встающий после <paramref name="previous"/> и перед <paramref name="next"/>.</returns>
    Position Between(Position? previous, Position? next);
}
