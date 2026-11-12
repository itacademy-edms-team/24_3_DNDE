using ErrorOr;

namespace Sets.Domain.Elements;

/// <summary>Термин карточки.</summary>
public readonly record struct CardTerm
{
    private CardTerm(string value) => Value = value;

    public string Value { get; }

    public static ErrorOr<CardTerm> Create(string? raw)
    {
        var trimmed = raw?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return ElementErrors.TermRequired;

        if (trimmed.Length > ElementConstraints.TermMaxLength)
            return ElementErrors.TermTooLong(trimmed.Length);

        return new CardTerm(trimmed);
    }

    /// <summary>Восстановление из БД (EF-конвертер и JSON payload): данные уже прошли валидацию при записи.</summary>
    public static CardTerm FromStorage(string value) => new(value);

    public override string ToString() => Value;
}
