using Media.Application.Objects;
using Media.Application.Storage;
using Media.Domain.Objects;
using Common.Infrastructure.Http;
using Microsoft.AspNetCore.Http;

namespace Media.Infrastructure.Http;

/// <summary>
/// Представление медиа-объекта для HTTP: состояние плюс подписанные ссылки на варианты.
/// Ссылки строятся в момент ответа, поэтому кешированный идемпотентный результат их не хранит.
/// </summary>
internal static class MediaHttpResponse
{
    public static async Task<IResult> FromAsync(
        MediaResponse media,
        IObjectStorage storage,
        CancellationToken cancellationToken)
    {
        var variants = new Dictionary<string, object>();

        foreach (var variant in media.Variants)
        {
            var url = await storage.CreateReadUrlAsync(
                variant.ObjectKey, MediaConstraints.ReadLinkLifetime, cancellationToken);
            if (url.IsError)
                return url.Errors.ToProblem();

            variants[variant.Variant.ToStorageName()] = new
            {
                url = url.Value,
                width = variant.Width,
                height = variant.Height,
                sizeBytes = variant.SizeBytes,
            };
        }

        return Results.Ok(new
        {
            mediaId = media.MediaId,
            status = media.Status.ToString().ToLowerInvariant(),
            failureCode = media.FailureCode,
            createdAt = media.CreatedAt,
            variants,
        });
    }
}
