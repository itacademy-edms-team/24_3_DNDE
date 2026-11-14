using Common.Application.Idempotency;
using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Media.Application.Abstractions;
using Media.Application.Storage;
using Media.Domain.Objects;

namespace Media.Application.Objects;

/// <param name="ContentType">Заявленный тип файла: должен быть из белого списка.</param>
/// <param name="SizeBytes">Заявленный размер: ограничивает политику загрузки.</param>
/// <param name="IdempotencyKey">Значение заголовка Idempotency-Key; проверяется на HTTP-слое.</param>
public sealed record InitUploadCommand(string? ContentType, long? SizeBytes, string IdempotencyKey)
    : IRequest<ErrorOr<InitUploadResponse>>
{
    /// <summary>
    /// Идентификатор операции в журнале идемпотентности (колонка <c>operation</c>).
    /// </summary>
    public const string Operation = "media.init";
}

/// <summary>Ответ на инициализацию: куда и с какими полями клиент отправляет файл.</summary>
/// <param name="MediaId">Идентификатор будущего файла.</param>
/// <param name="Url">Endpoint хранилища для POST.</param>
/// <param name="Fields">Подписанные поля формы.</param>
/// <param name="ObjectKey">Ключ, по которому сервер найдёт объект при подтверждении.</param>
/// <param name="ExpiresInSeconds">Сколько живёт политика.</param>
public sealed record InitUploadResponse(
    Guid MediaId,
    string Url,
    IReadOnlyDictionary<string, string> Fields,
    string ObjectKey,
    int ExpiresInSeconds);

/// <summary>
/// Первый шаг загрузки: создаёт `pending` и выдаёт политику, по которой клиент отправит байты напрямую в хранилище.
/// Через модуль файл не идёт (ADR 0037).
/// </summary>
public sealed class InitUploadHandler(
    IMediaObjectRepository repository,
    IObjectStorage storage,
    IMediaIdempotencyStore idempotency,
    ICurrentUser currentUser,
    TimeProvider clock)
    : IRequestHandler<InitUploadCommand, ErrorOr<InitUploadResponse>>
{
    public async Task<ErrorOr<InitUploadResponse>> Handle(
        InitUploadCommand command,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return MediaErrors.UserUnresolved;

        if (!MediaContentTypes.TryNormalize(command.ContentType, out var contentType))
            return MediaErrors.ContentTypeUnsupported;

        if (command.SizeBytes is not { } declaredSize || declaredSize is < MediaConstraints.MinUploadBytes or > MediaConstraints.MaxUploadBytes)
            return MediaErrors.FileTooLarge;

        return await idempotency.ExecuteAsync<InitUploadResponse>(
            ownerId,
            command.IdempotencyKey,
            operation: InitUploadCommand.Operation,
            requestHash: InitUploadRequestHash.Compute(contentType, declaredSize),
            operationBody: async token =>
            {
                // Проверяем лимит незавершённых загрузок: слот занимают только те, чьё окно ещё живо.
                var now = clock.GetUtcNow().UtcDateTime;

                // Истёкшие загрузки подтвердить уже нельзя - переводим их в failed до подсчёта, чтобы не ждали чистки.
                await repository.MarkExpiredUploadsAsync(ownerId, now, token);

                var pending = await repository.CountPendingAsync(ownerId, now, token);
                if (pending >= MediaConstraints.MaxPendingPerOwner)
                    return MediaErrors.PendingLimitReached;

                var created = MediaObject.Create(ownerId, contentType, now);
                if (created.IsError)
                    return created.Errors;

                var media = created.Value;
                var objectKey = ObjectKey.Upload(media.Id);

                var policy = await storage.CreateUploadPolicyAsync(
                    objectKey.Value,
                    contentType,
                    MediaConstraints.MaxUploadBytes,
                    now,
                    token);
                if (policy.IsError)
                    return policy.Errors;

                repository.Add(media);
                await repository.SaveChangesAsync(token);

                return new InitUploadResponse(
                    media.Id,
                    policy.Value.Url,
                    policy.Value.Fields,
                    policy.Value.ObjectKey,
                    (int)Math.Round((policy.Value.ExpiresAt - now).TotalSeconds));
            },
            cancellationToken);
    }
}
