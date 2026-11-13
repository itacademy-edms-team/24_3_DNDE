namespace Sets.Domain.Elements;

/// <summary>
/// Назначение позиции элементу набора:
/// результат пересчёта порядка в домене, который потом инфраструктура применяет к строкам.
/// </summary>
public readonly record struct ElementPosition(Guid ElementId, int Position);
