using System.Text.Json.Serialization;

namespace Sets.Domain.Elements;

/// <summary>
/// Тип элемента: задаёт состав документного поля и правила его проверки.
/// Задаётся при создании и затем не меняется - смена типа означает новый элемент (онтология §6.4).
/// </summary>
public enum ElementType
{
    /// <summary>Флеш-карточка: термин и определение (CREATE-01).</summary>
    Flashcard = 1,
}

/// <summary>
/// Отображаемое имя типа для контракта и хранения: стабильная строка, не зависящая от имён в коде.
/// </summary>
public static class ElementTypeNames
{
    public const string Flashcard = "flashcard";

    public static bool TryParse(string? value, out ElementType type)
    {
        type = default;
        if (!string.Equals(value?.Trim(), Flashcard, StringComparison.OrdinalIgnoreCase))
            return false;

        type = ElementType.Flashcard;
        return true;
    }

    public static string ToContractValue(this ElementType type) => type switch
    {
        ElementType.Flashcard => Flashcard,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown element type."),
    };
}
