using System.Collections.Concurrent;

namespace Common.Application.DomainEvents;

/// <summary>
/// Реестр «стабильное имя события -> CLR-тип»: по нему outbox-воркер десериализует payload
/// в конкретное событие. Заполняется при регистрации обработчиков (ADR 0040, §9).
/// </summary>
public static class DomainEventTypeRegistry
{
    private static readonly ConcurrentDictionary<string, Type> Types = new(StringComparer.Ordinal);

    /// <summary>Связывает стабильное имя события с его CLR-типом. Повторная регистрация того же имени - ошибка.</summary>
    public static void Register(string name, Type eventType)
    {
        if (!Types.TryAdd(name, eventType) && Types[name] != eventType)
            throw new InvalidOperationException($"Имя доменного события '{name}' зарегистрировано для разных типов.");
    }

    /// <summary>Возвращает CLR-тип события по стабильному имени или null, если имя неизвестно.</summary>
    public static Type? Resolve(string name) => Types.GetValueOrDefault(name);
}
