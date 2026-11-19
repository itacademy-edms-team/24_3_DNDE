using System;

namespace Sets.Domain.Elements;

/// <summary>
/// Представляет позицию элемента в наборе.
/// </summary>
/// <remarks>
/// <para>Позиция - строковый ранг: между любыми двумя значениями можно получить третье,
/// не меняя соседей.</para>
/// <para>Сравнение порядковое (ordinal), поэтому строки в базе сортируются побайтово -
/// столбец объявлен с <c>COLLATE "C"</c>.</para>
/// </remarks>
public readonly record struct Position : IComparable<Position>
{
    private Position(string value) => Value = value;

    /// <summary>
    /// Возвращает строковое значение ранга.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Восстанавливает позицию из уже проверенного значения.
    /// </summary>
    /// <param name="value">Ранг из генератора или из хранилища; считается корректным.</param>
    public static Position FromStorage(string value) => new(value);

    /// <summary>
    /// Сравнивает позиции по порядковому значению строки.
    /// </summary>
    /// <param name="other">Позиция для сравнения.</param>
    /// <returns>Отрицательное число, ноль или положительное число.</returns>
    public int CompareTo(Position other) => string.CompareOrdinal(Value, other.Value);

    /// <summary>
    /// Возвращает строковое значение ранга.
    /// </summary>
    public override string ToString() => Value;
}
