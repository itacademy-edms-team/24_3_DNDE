using ErrorOr;

namespace Sets.Domain.Sets;

/// <summary>
/// Описание набора.
/// Пустая строка означает «описания нет».
/// </summary>
public readonly record struct SetDescription
{
    private SetDescription(string value) => Value = value;

    public string Value { get; }

    public static ErrorOr<SetDescription> Create(string raw)
    {
        var trimmed = raw.Trim();
        if (trimmed.Length > SetConstraints.DescriptionMaxLength)
            return SetErrors.DescriptionTooLong(trimmed.Length);

        return new SetDescription(trimmed);
    }

    /// <summary>Восстановление из БД (EF-конвертер): данные уже прошли валидацию при записи.</summary>
    public static SetDescription FromStorage(string value) => new(value);

    public override string ToString() => Value;
}
