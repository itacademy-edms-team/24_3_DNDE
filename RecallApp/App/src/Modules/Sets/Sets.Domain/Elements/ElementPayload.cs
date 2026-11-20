namespace Sets.Domain.Elements;

/// <summary>
/// Предоставляет базовый класс для содержимого элемента набора.
/// </summary>
/// <remarks>
/// <para>Состав полей задаёт тип элемента (ADR 0015): набор, тип и позиция от payload не зависят.</para>
/// <para>Новый вид элемента - это новый наследник этого класса.</para>
/// <para>Схема таблицы и маппинг EF не меняются: специфика вида хранится в столбце <c>payload</c> (jsonb).</para>
/// <para>Новый вид нужно зарегистрировать в <see cref="ElementPayloadJson"/>,
/// <see cref="ElementTypeNames"/> и <see cref="ElementType"/>.</para>
/// </remarks>
public abstract record ElementPayload
{
    /// <summary>
    /// Возвращает тип содержимого, которому принадлежит payload.
    /// </summary>
    /// <remarks>
    /// Служит дискриминатором: значение попадает в столбец <c>type</c>,
    /// по которой при чтении выбирается конкретный класс для содержимого.
    /// </remarks>
    public abstract ElementType Type { get; }
}
