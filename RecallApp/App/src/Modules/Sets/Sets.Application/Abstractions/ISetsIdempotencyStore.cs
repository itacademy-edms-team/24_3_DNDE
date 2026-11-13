using Common.Application.Idempotency;

namespace Sets.Application.Abstractions;

/// <summary>
/// Журнал идемпотентности модуля Sets. Отдельный интерфейс-маркер нужен, потому что
/// реализации у модулей свои (своя схема БД), а общий <see cref="IIdempotencyStore"/>
/// не может быть зарегистрирован дважды в одном контейнере.
/// </summary>
public interface ISetsIdempotencyStore : IIdempotencyStore;
