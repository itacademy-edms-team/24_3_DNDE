using ErrorOr;

namespace Sets.Domain.Sets;

/// <summary>
/// Название набора.
/// </summary>
public readonly record struct SetTitle
{
    private SetTitle(string value) => Value = value;

    public string Value { get; }

    public static ErrorOr<SetTitle> Create(string? raw)
    {
        var trimmed = raw?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return SetErrors.TitleRequired;

        if (trimmed.Length > SetConstraints.TitleMaxLength)
            return SetErrors.TitleTooLong(trimmed.Length);

        return new SetTitle(trimmed);
    }

    /// <summary>Восстановление из БД (EF-конвертер): данные уже прошли валидацию при записи.</summary>
    public static SetTitle FromStorage(string value) => new(value);

    public override string ToString() => Value;
}
