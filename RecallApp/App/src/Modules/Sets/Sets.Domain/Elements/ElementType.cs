namespace Sets.Domain.Elements;

/// <summary>
/// Указывает тип содержимого элемента набора.
/// </summary>
/// <remarks>
/// <para>Тип задаёт состав документного поля и правила его проверки (онтология §6.4).</para>
/// <para>Задаётся при создании и дальше не меняется: смена типа означала бы, что это новый элемент,
/// а не правка существующего.</para>
/// <para>Числовые значения закреплены явно, чтобы добавление новых типов не сдвигало уже существующие.</para>
/// </remarks>
public enum ElementType
{
    /// <summary>Флеш-карточка: термин и определение.</summary>
    Flashcard = 1,
}

/// <summary>
/// Предоставляет контрактные имена для типов элемента.
/// </summary>
public static class ElementTypeNames
{
    /// <summary>
    /// Контрактное имя типа <c>флеш-карточка</c>.
    /// </summary>
    public const string Flashcard = "flashcard";

    /// <summary>
    /// Пытается разобрать контрактное имя типа элемента.
    /// </summary>
    /// <param name="value">Разбираемое имя.</param>
    /// <param name="type">Тип элемента, полученный при успешном разборе.</param>
    /// <returns><see langword="true"/>, если разбор завершился успешно, иначе <see langword="false"/>.</returns>
    public static bool TryParse(string? value, out ElementType type)
    {
        type = default;
        if (!string.Equals(value?.Trim(), Flashcard, StringComparison.OrdinalIgnoreCase))
            return false;

        type = ElementType.Flashcard;
        return true;
    }

    /// <summary>
    /// Возвращает контрактное имя типа элемента.
    /// </summary>
    /// <param name="type">Тип элемента.</param>
    /// <returns>Контрактное имя типа.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Передан неизвестный тип элемента.</exception>
    public static string ToContractValue(this ElementType type) => type switch
    {
        ElementType.Flashcard => Flashcard,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown element type."),
    };
}
