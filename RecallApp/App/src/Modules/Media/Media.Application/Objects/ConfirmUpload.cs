using Common.Application.Provisioning;
using ErrorOr;
using MediatR;
using Media.Application.Abstractions;
using Media.Application.Images;
using Media.Application.Storage;
using Media.Domain.Objects;

namespace Media.Application.Objects;

/// <param name="IdempotencyKey">Значение заголовка Idempotency-Key; проверяется на HTTP-слое.</param>
public sealed record ConfirmUploadCommand(Guid MediaId, string IdempotencyKey)
    : IRequest<ErrorOr<MediaResponse>>
{
    /// <summary>Идентификатор операции в журнале идемпотентности: стабилен, уже записан в БД.</summary>
    public const string Operation = "media.confirm";
}

/// <summary>Сведения о варианте файла. Ключ нужен для подписи ссылки, клиенту он не отдаётся.</summary>
public sealed record MediaVariantInfo(MediaVariantName Variant, int Width, int Height, long SizeBytes, string ObjectKey);

/// <summary>Состояние файла и его готовые варианты.</summary>
public sealed record MediaResponse(
    Guid MediaId,
    MediaStatus Status,
    string? FailureCode,
    IReadOnlyList<MediaVariantInfo> Variants)
{
    public static MediaResponse From(MediaObject media) => new(
        media.Id,
        media.Status,
        media.FailureCode,
        media.Variants
            .OrderBy(variant => variant.Variant)
            .Select(variant => new MediaVariantInfo(variant.Variant, variant.Width, variant.Height, variant.SizeBytes, variant.ObjectKey.Value))
            .ToArray());
}

/// <summary>
/// Второй шаг загрузки: проверяет, что объект действительно долетел, обрабатывает изображение,
/// записывает варианты и переводит файл в `ready`. Работа идёт вне транзакции —
/// ключ держится арендой (онтология Media §6.6).
/// </summary>
public sealed class ConfirmUploadHandler(
    ICurrentUser currentUser,
    IMediaObjectRepository repository,
    IObjectStorage storage,
    IImageProcessor processor,
    IUploadIdempotencyStore idempotency,
    TimeProvider clock)
    : IRequestHandler<ConfirmUploadCommand, ErrorOr<MediaResponse>>
{
    private const string WebpContentType = "image/webp";

    public async Task<ErrorOr<MediaResponse>> Handle(
        ConfirmUploadCommand command,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } ownerId)
            return MediaErrors.UserUnresolved;

        // Владение проверяется до идемпотентности: чужой файл не должен попадать в журнал ключей.
        var media = await repository.FindAsync(command.MediaId, cancellationToken);
        if (media is null || media.OwnerId != ownerId)
            return MediaErrors.NotFound;

        return await idempotency.ExecuteAsync<MediaResponse>(
            ownerId,
            command.IdempotencyKey,
            operation: ConfirmUploadCommand.Operation,
            requestHash: ConfirmUploadRequestHash.Compute(command.MediaId),
            operationBody: token => ConfirmAsync(media, ownerId, token),
            cancellationToken);
    }
    
    private async Task<ErrorOr<MediaResponse>> ConfirmAsync(MediaObject media, Guid ownerId, CancellationToken token)
    {
        var now = clock.GetUtcNow().UtcDateTime;

        var confirmable = media.EnsureConfirmable(now);
        if (confirmable.IsError)
            return confirmable.Errors;

        var uploadKey = ObjectKey.Upload(media.Id).Value;

        // Факт загрузки проверяем HEAD-ом: успешный POST от хранилища ничего не доказывает.
        var stored = await storage.HeadAsync(uploadKey, token);
        if (stored is null)
            return await FailAsync(media, MediaErrors.ObjectMissing, token);

        if (stored.SizeBytes is <= 0 or > MediaConstraints.MaxUploadBytes)
            return await FailAsync(media, MediaErrors.FileTooLarge, token);

        var content = await storage.ReadAsync(uploadKey, MediaConstraints.MaxUploadBytes, token);
        if (content.IsError)
            return content.Errors;

        var processed = await processor.ProcessAsync(content.Value, token);
        if (processed.IsError)
            return await FailAsync(media, processed.FirstError, token);

        var image = processed.Value;
        var variants = new List<MediaVariant>(MediaConstraints.VariantsPerObject);

        foreach (var processedVariant in image.Variants)
        {
            var variant = MediaVariant.Create(
                media.Id,
                ownerId,
                processedVariant.Variant,
                processedVariant.Width,
                processedVariant.Height,
                processedVariant.Content.Length,
                now);
            if (variant.IsError)
                return await FailAsync(media, variant.FirstError, token);

            var written = await storage.WriteAsync(
                variant.Value.ObjectKey.Value, processedVariant.Content, WebpContentType, token);
            if (written.IsError)
                return written.Errors;

            variants.Add(variant.Value);
        }

        var ready = media.MarkReady(
            image.Format, image.Width, image.Height, stored.SizeBytes, variants, now);
        if (ready.IsError)
            return ready.Errors;

        await repository.SaveChangesAsync(token);

        // Исходник больше не нужен: сбой удаления не должен превращать успешную обработку в ошибку.
        await storage.DeleteAsync(uploadKey, token);

        return MediaResponse.From(media);
    }

    /// <summary>Делает пометку, что файл окончательно непригоден: фиксируем причину, чтобы клиент увидел её в статусе.</summary>
    private async Task<ErrorOr<MediaResponse>> FailAsync(MediaObject media, Error error, CancellationToken token)
    {
        var failed = media.MarkFailed(error, clock.GetUtcNow().UtcDateTime);
        if (failed.IsError)
            return failed.Errors;

        await repository.SaveChangesAsync(token);
        return error;
    }
}
