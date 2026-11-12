using ErrorOr;

namespace Sets.Domain.Elements;

/// <summary>Определение флеш-карточки.</summary>
public readonly record struct CardDefinition
{
    private CardDefinition(string value) => Value = value;

    public string Value { get; }

    public static ErrorOr<CardDefinition> Create(string? raw)
    {
        var trimmed = raw?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return ElementErrors.DefinitionRequired;

        if (trimmed.Length > ElementConstraints.DefinitionMaxLength)
            return ElementErrors.DefinitionTooLong(trimmed.Length);

        return new CardDefinition(trimmed);
    }

    /// <summary>Восстановление из БД (EF-конвертер и JSON payload): данные уже прошли валидацию при записи.</summary>
    public static CardDefinition FromStorage(string value) => new(value);

    public override string ToString() => Value;
}
