using ErrorOr;

namespace Sets.Domain.Elements;

/// <summary>
/// Представляет позицию элемента в наборе.
/// </summary>
/// <remarks>
/// Позиция - строковый ранг.
/// Сравнение - порядковое (ordinal).
/// </remarks>
public readonly record struct Position : IComparable<Position>
{
    private Position(string value) => Value = value;

    /// <summary>
    /// Возвращает строковое значение ранга.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Создаёт позицию из строкового ранга.
    /// </summary>
    /// <param name="value">Ранг в формате «бакет|значение».</param>
    /// <returns>Позиция либо ошибка <see cref="ElementErrors.PositionInvalid"/>.</returns>
    public static ErrorOr<Position> Create(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? ElementErrors.PositionInvalid(value)
            : new Position(value);

    /// <summary>
    /// Восстанавливает позицию из уже проверенного значения: курсора или строки из хранилища.
    /// </summary>
    /// <param name="value">Ранг, прошедший проверку ранее.</param>
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
