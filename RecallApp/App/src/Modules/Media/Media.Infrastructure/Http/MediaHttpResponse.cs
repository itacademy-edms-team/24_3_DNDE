using Media.Application.Objects;
using Media.Application.Storage;
using Media.Domain.Objects;
using Common.Infrastructure.Http;
using Microsoft.AspNetCore.Http;

namespace Media.Infrastructure.Http;

/// <summary>
/// Преобразует представление медиа-объекта в HTTP-ответ.
/// </summary>
/// <remarks>
/// Ссылки на варианты подписываются при каждой отдаче:
/// в кеше идемпотентности лежат ключи вариантов, а не подписанные адреса.
/// </remarks>
internal static class MediaHttpResponse
{
    /// <summary>
    /// Формирует HTTP-ответ с состоянием и подписанными ссылками на варианты.
    /// </summary>
    /// <param name="media">Представление медиа-объекта из прикладного слоя.</param>
    /// <param name="storage">Объектное хранилище для подписи ссылок.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>HTTP-ответ либо ProblemDetails, если не удалось подписать хотя бы одну ссылку.</returns>
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
