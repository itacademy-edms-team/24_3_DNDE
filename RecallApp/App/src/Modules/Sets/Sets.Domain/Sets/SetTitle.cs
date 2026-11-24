using ErrorOr;

namespace Sets.Domain.Sets;

/// <summary>
/// Представляет название набора.
/// </summary>
public readonly record struct SetTitle
{
    private SetTitle(string value) => Value = value;

    /// <summary>
    /// Возвращает строковое значение названия.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Создаёт название из сырой строки, нормализуя её.
    /// </summary>
    /// <param name="raw">Сырое значение; <see langword="null"/> и пробельная строка недопустимы.</param>
    /// <returns>
    /// Нормализованное название либо ошибка:
    /// <see cref="SetErrors.TitleRequired"/>, <see cref="SetErrors.TitleTooLong"/>.
    /// </returns>
    public static ErrorOr<SetTitle> Create(string? raw)
    {
        var trimmed = raw?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return SetErrors.TitleRequired;

        if (trimmed.Length > SetConstraints.TitleMaxLength)
            return SetErrors.TitleTooLong(trimmed.Length);

        return new SetTitle(trimmed);
    }

    /// <summary>
    /// Восстанавливает название из сохранённого значения.
    /// </summary>
    /// <param name="value">Значение из хранилища; считается уже прошедшим проверку при записи.</param>
    public static SetTitle FromStorage(string value) => new(value);

    /// <summary>
    /// Возвращает название в виде строки.
    /// </summary>
    public override string ToString() => Value;
}
