using ErrorOr;

namespace Sets.Domain.Sets;

/// <summary>
/// Представляет описание набора.
/// </summary>
/// <remarks>
/// Отсутствие описания выражается пустым значением <see cref="Empty"/>:
/// <see langword="null"/> в домен не попадает,
/// а пробельная строка после нормализации становится <see cref="Empty"/>.
/// </remarks>
public readonly record struct SetDescription
{
    private SetDescription(string value) => Value = value;
    
    /// <summary>
    /// Возвращает пустое описание.
    /// </summary>
    public static SetDescription Empty => new(string.Empty);

    /// <summary>
    /// Возвращает строковое значение описания.
    /// </summary>
    public string Value { get; }
    
    /// <summary>
    /// Создаёт описание из сырой строки, нормализуя её.
    /// </summary>
    /// <param name="raw">Строковое представление описания; <see langword="null"/>
    /// и пробельная строка означают, что описания нет.</param>
    /// <returns>
    /// Нормализованное описание либо ошибка <see cref="SetErrors.DescriptionTooLong"/>.
    /// </returns>
    public static ErrorOr<SetDescription> Create(string? raw)
    {
        var trimmed = raw?.Trim();
        
        if (string.IsNullOrEmpty(trimmed))
            return Empty;
        
        if (trimmed.Length > SetConstraints.DescriptionMaxLength)
            return SetErrors.DescriptionTooLong(trimmed.Length);

        return new SetDescription(trimmed);
    }

    /// <summary>
    /// Восстанавливает описание из сохранённого значения.
    /// </summary>
    /// <param name="value">Значение из хранилища; считается уже прошедшим проверку при записи.</param>
    public static SetDescription FromStorage(string value) => new(value);

    /// <summary>
    /// Возвращает описание в виде строки.
    /// </summary>
    public override string ToString() => Value;
}
