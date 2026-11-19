using ErrorOr;

namespace Sets.Domain.Elements;

/// <summary>
/// Представляет термин флеш-карточки.
/// </summary>
public readonly record struct CardTerm
{
    private CardTerm(string value) => Value = value;

    /// <summary>
    /// Возвращает значение термина в виде строки.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Создаёт термин из сырой строки, нормализуя её.
    /// </summary>
    /// <param name="raw">Сырая строка.</param>
    /// <returns>
    /// Новый термин или ошибка:
    /// <see cref="ElementErrors.TermRequired"/>, <see cref="ElementErrors.TermTooLong"/>.
    /// </returns>
    public static ErrorOr<CardTerm> Create(string? raw)
    {
        var trimmed = raw?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return ElementErrors.TermRequired;

        if (trimmed.Length > ElementConstraints.TermMaxLength)
            return ElementErrors.TermTooLong(trimmed.Length);

        return new CardTerm(trimmed);
    }

    /// <summary>
    /// Восстанавливает термин из сохранённого значения.
    /// </summary>
    /// <param name="value">Значение из хранилища; считается уже прошедшим проверку при записи.</param>
    /// <returns>Термин.</returns>
    public static CardTerm FromStorage(string value) => new(value);

    /// <summary>
    /// Возвращает представление термина в виде строки.
    /// </summary>
    public override string ToString() => Value;
}
