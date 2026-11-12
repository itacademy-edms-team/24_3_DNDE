namespace Sets.Domain.Elements;

/// <summary>
/// Документное содержимое элемента набора; состав полей задаёт тип элемента (ADR 0015).
/// Реляционная часть (набор, тип, позиция) от payload не зависит, поэтому новый формат -
/// это новый наследник от этого типа, а не правка <see cref="Element"/> и EF-конфигурации.
/// </summary>
public abstract record ElementPayload
{
    /// <summary>Тип, которому принадлежит payload; реляционный дискриминатор выводится из него.</summary>
    public abstract ElementType Type { get; }
}
