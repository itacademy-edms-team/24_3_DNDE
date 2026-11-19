using System.Collections.Concurrent;

namespace Common.Application.DomainEvents;

/// <summary>
/// Предоставляет реестр "стабильное имя события" -> тип события».
/// </summary>
/// <remarks>
/// По нему outbox-воркер десериализует payload в конкретное событие.
/// Заполняется при регистрации обработчиков (ADR 0040, §9).
/// </remarks>
public static class DomainEventTypeRegistry
{
    private static readonly ConcurrentDictionary<string, Type> Types = new(StringComparer.Ordinal);

    /// <summary>
    /// Связывает стабильное имя события с его типом.
    /// </summary>
    /// <param name="name">Стабильное имя события.</param>
    /// <param name="eventType">Тип события.</param>
    /// <exception cref="InvalidOperationException">Имя уже зарегистрировано для другого типа.</exception>
    public static void Register(string name, Type eventType)
    {
        if (!Types.TryAdd(name, eventType) && Types[name] != eventType)
            throw new InvalidOperationException($"Domain event name '{name}' is registered for different types.");
    }

    /// <summary>
    /// Возвращает тип события по стабильному имени.
    /// </summary>
    /// <param name="name">Стабильное имя события.</param>
    /// <returns>Тип события или <see langword="null"/>, если имя неизвестно.</returns>
    public static Type? Resolve(string name) => Types.GetValueOrDefault(name);
}
