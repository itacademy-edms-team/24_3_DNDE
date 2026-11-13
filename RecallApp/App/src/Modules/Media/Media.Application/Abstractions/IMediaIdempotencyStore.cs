using Common.Application.Idempotency;

namespace Media.Application.Abstractions;

/// <summary>
/// Журнал идемпотентности модуля Media. Отдельный интерфейс-маркер нужен, потому что
/// реализации у модулей свои (своя схема БД), а общий <see cref="IIdempotencyStore"/>
/// не может быть зарегистрирован дважды в одном контейнере.
/// </summary>
public interface IMediaIdempotencyStore : IIdempotencyStore;
