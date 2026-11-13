using ErrorOr;

namespace Common.Application.DomainEvents;

/// <summary>
/// Сигнал о том, что один или несколько обработчиков не смогли применить событие.
/// Бросается только внутри MediatR-моста и там же превращается обратно в <see cref="ErrorOr"/>,
/// поэтому в штатном коде исключением не считается (ADR 0038).
/// </summary>
public sealed class DomainEventDispatchException(IReadOnlyList<Error> errors)
    : Exception(string.Join("; ", errors.Select(error => error.Code)))
{
    public IReadOnlyList<Error> Errors { get; } = errors;
}
