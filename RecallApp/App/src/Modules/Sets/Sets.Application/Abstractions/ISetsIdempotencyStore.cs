using Common.Application.Idempotency;

namespace Sets.Application.Abstractions;

/// <summary>
/// Определяет журнал идемпотентности модуля Sets.
/// </summary>
/// <remarks>
/// Отдельный интерфейс нужен, потому что реализация у каждого модуля своя - со своей схемой БД,
/// - а один общий <see cref="IIdempotencyStore"/> в контейнере нельзя зарегистрировать дважды.
/// </remarks>
public interface ISetsIdempotencyStore : IIdempotencyStore;
