using ErrorOr;

namespace Sets.Domain.Elements;

/// <summary>
/// Представляет определение флеш-карточки.
/// </summary>
public readonly record struct CardDefinition
{
    private CardDefinition(string value) => Value = value;

    /// <summary>
    /// Возвращает значение определения в виде строки.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Создаёт определение из сырой строки, нормализуя её.
    /// </summary>
    /// <param name="raw">Сырая строка; пробелы по краям обрезаются.</param>
    /// <returns>
    /// Новое определение или ошибка:
    /// <see cref="ElementErrors.DefinitionRequired"/>, <see cref="ElementErrors.DefinitionTooLong"/>.
    /// </returns>
    public static ErrorOr<CardDefinition> Create(string? raw)
    {
        var trimmed = raw?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return ElementErrors.DefinitionRequired;

        if (trimmed.Length > ElementConstraints.DefinitionMaxLength)
            return ElementErrors.DefinitionTooLong(trimmed.Length);

        return new CardDefinition(trimmed);
    }

    /// <summary>
    /// Восстанавливает определение из сохранённого значения.
    /// </summary>
    /// <param name="value">Значение из хранилища; считается уже прошедшим проверку при записи.</param>
    /// <returns>Определение из хранилища.</returns>
    public static CardDefinition FromStorage(string value) => new(value);

    /// <summary>
    /// Возвращает представление определения в виде строки.
    /// </summary>
    public override string ToString() => Value;
}
