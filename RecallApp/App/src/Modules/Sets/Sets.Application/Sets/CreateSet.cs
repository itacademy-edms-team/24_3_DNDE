using Common.Application.Idempotency;
using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Sets.Application.Abstractions;
using Sets.Domain.Sets;

namespace Sets.Application.Sets;

/// <param name="Title">Название; отсутствие или пустая строка - ошибка Set.TitleRequired.</param>
/// <param name="Description">Описание необязательно.</param>
/// <param name="IdempotencyKey">Значение заголовка Idempotency-Key; проверяется на HTTP-слое.</param>
public sealed record CreateSetCommand(string? Title, string? Description, string IdempotencyKey)
    : IRequest<ErrorOr<SetResponse>>
{
    /// <summary>
    /// Идентификатор операции в журнале идемпотентности (колонка <c>operation</c>).
    /// Значение стабильно: оно уже записано в БД и не должно меняться при переименовании класса.
    /// </summary>
    public const string Operation = "sets.create";
}

public sealed class CreateSetHandler(
    ISetRepository repository,
    ISetsIdempotencyStore idempotency,
    ICurrentUser currentUser,
    TimeProvider clock)
    : IRequestHandler<CreateSetCommand, ErrorOr<SetResponse>>
{
    public async Task<ErrorOr<SetResponse>> Handle(CreateSetCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return SetErrors.UserUnresolved;

        // Повтор с тем же ключом и телом вернёт сохранённый ответ, с другим телом - 409 (ADR 0038, ontology §8).
        return await idempotency.ExecuteAsync<SetResponse>(
            ownerId,
            command.IdempotencyKey,
            operation: CreateSetCommand.Operation,
            requestHash: CreateSetRequestHash.Compute(command.Title, command.Description),
            operationBody: async token =>
            {
                var created = Set.Create(ownerId, command.Title, command.Description, clock.GetUtcNow().UtcDateTime);
                if (created.IsError)
                    return created.Errors;

                repository.Add(created.Value);
                await repository.SaveChangesAsync(token);
                return SetResponse.From(created.Value);
            }, cancellationToken);
    }
}
