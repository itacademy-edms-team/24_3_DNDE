using Common.Application.Idempotency;

namespace Media.Application.Abstractions;

/// <summary>
/// Определяет хранилище идемпотентности модуля Media.
/// </summary>
/// <remarks>
/// Интерфейс-маркер: реализации у модулей свои, потому что каждая пишет в свою схему БД,
/// а общий <see cref="IIdempotencyStore"/> нельзя зарегистрировать дважды в одном DI-контейнере.
/// </remarks>
public interface IMediaIdempotencyStore : IIdempotencyStore;
