using Amazon.S3;
using Common.Application.Behaviors;
using Common.Application.DomainEvents;
using Common.Infrastructure.Http;
using MediatR;
using Media.Application.Abstractions;
using Media.Application.Images;
using Media.Application.Objects;
using Media.Application.Storage;
using Media.Contracts;
using Media.Infrastructure.Contracts;
using Media.Infrastructure.Data;
using Media.Infrastructure.Data.Cleanup;
using Media.Infrastructure.Data.Idempotency;
using Media.Infrastructure.Http;
using Media.Infrastructure.Images;
using Media.Infrastructure.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Media.Infrastructure;

/// <summary>
/// Предоставляет композицию модуля Media:
/// регистрацию сервисов, маршруты, миграции и проверку связности.
/// </summary>
/// <remarks>
/// Единственная публичная точка модуля наряду с контрактом.
/// </remarks>
public static class MediaModule
{
    /// <summary>
    /// Регистрирует модуль на общем runtime-пуле соединений.
    /// </summary>
    /// <remarks>
    /// Квота соединений резервируется хостом через <c>ConnectionPoolBudget</c> до вызова.
    /// </remarks>
    /// <param name="services">Коллекция сервисов хоста.</param>
    /// <param name="dataSource">Общий пул соединений приложения.</param>
    /// <returns>Та же коллекция для цепочки регистраций.</returns>
    public static IServiceCollection AddMedia(this IServiceCollection services, NpgsqlDataSource dataSource)
    {
        services.AddDbContext<MediaDbContext>(options => options.UseNpgsql(dataSource,
            pg => pg.MigrationsHistoryTable("__EFMigrationsHistory", "media")));

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(InitUploadCommand).Assembly);
            configuration.AddCommandValidation();
        });
        services.AddCommandValidators(typeof(InitUploadCommand));

        services.AddScoped<IMediaObjectRepository, MediaObjectRepository>();
        services.AddScoped<IMediaIdempotencyStore, IdempotencyStore>();
        services.AddScoped<IUploadIdempotencyStore, LeasedIdempotencyStore>();
        services.AddScoped<IMediaReferenceRepository, MediaReferenceRepository>();
        services.AddScoped<IMediaLinkReader, MediaLinkReader>();
        services.AddDomainEventHandlers(typeof(MediaModule).Assembly);

        // Контракт для других модулей: проверка готовности и выдача подписанных ссылок.
        services.AddScoped<IMediaReadiness, MediaReadiness>();
        services.AddScoped<IMediaLinksQuery, MediaLinksQuery>();
        services.AddSingleton<IImageProcessor, SkiaImageProcessor>();

        services.AddScoped<MediaCleaner>();
        services.AddOptions<MediaCleanupOptions>().BindConfiguration("Media:Cleanup");
        services.AddHostedService<MediaCleanupService>();

        services.AddOptions<S3Options>()
            .BindConfiguration(S3Options.SectionName)
            .Validate(
                options => options.Missing().Count == 0,
                "Media:S3: не заданы обязательные параметры (Endpoint, Region, Bucket, AccessKeyId, SecretKey).")
            .ValidateOnStart();
        services.AddSingleton<IAmazonS3>(provider =>
            S3ObjectStorage.CreateClient(provider.GetRequiredService<IOptions<S3Options>>().Value));
        services.AddSingleton<IObjectStorage>(provider => new S3ObjectStorage(
            provider.GetRequiredService<IAmazonS3>(),
            provider.GetRequiredService<IOptions<S3Options>>().Value.Bucket));

        return services;
    }

    /// <summary>
    /// Регистрирует маршруты модуля.
    /// </summary>
    /// <param name="endpoints">Построитель маршрутов хоста.</param>
    /// <returns>Тот же построитель для цепочки.</returns>
    public static IEndpointRouteBuilder MapMedia(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(MediaRoute.Group).WithTags("Media").RequireAuthorization();

        group.MapPost(MediaRoute.Uploads, async (InitUploadRequest request, HttpContext http, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new InitUploadCommand(request.ContentType, request.SizeBytes,
                    IdempotencyKeyFilter.GetCanonicalKey(http)),
                cancellationToken);

            return result.IsError
                ? result.Errors.ToProblem()
                : Results.Created($"{MediaRoute.Group}/{result.Value.MediaId}", result.Value);
        }).AddEndpointFilter<IdempotencyKeyFilter>();

        group.MapPost(MediaRoute.ConfirmUpload, async (
            Guid mediaId,
            HttpContext http,
            ISender sender,
            IObjectStorage storage,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new ConfirmUploadCommand(mediaId, IdempotencyKeyFilter.GetCanonicalKey(http)),
                cancellationToken);

            if (result.IsError)
                return result.Errors.ToProblem();

            return await MediaHttpResponse.FromAsync(result.Value, storage, cancellationToken);
        }).AddEndpointFilter<IdempotencyKeyFilter>();

        group.MapGet(MediaRoute.MediaItem, async (
            Guid mediaId,
            ISender sender,
            IObjectStorage storage,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetMediaQuery(mediaId), cancellationToken);
            return result.IsError
                ? result.Errors.ToProblem()
                : await MediaHttpResponse.FromAsync(result.Value, storage, cancellationToken);
        });

        group.MapDelete(MediaRoute.MediaItem, async (Guid mediaId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeleteMediaCommand(mediaId), cancellationToken);
            return result.IsError ? result.Errors.ToProblem() : Results.NoContent();
        });

        group.MapPost(MediaRoute.Links, async (
            GetMediaLinksRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetMediaLinksQuery(request.MediaIds ?? []),
                cancellationToken);

            return result.IsError
                ? result.Errors.ToProblem()
                : Results.Ok(result.Value);
        });

        return endpoints;
    }

    /// <summary>Применяет миграции модуля.</summary>
    /// <param name="connectionString">Строка подключения пользователя-владельца схемы media.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public static async Task MigrateMedia(string connectionString, CancellationToken cancellationToken = default)
    {
        var options = new DbContextOptionsBuilder<MediaDbContext>().UseNpgsql(connectionString,
            pg => pg.MigrationsHistoryTable("__EFMigrationsHistory", "media")).Options;
        await using var dbContext = new MediaDbContext(options);
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    /// <summary>
    /// Проверяет, доступна ли схема модуля.
    /// </summary>
    /// <param name="services">Провайдер сервисов хоста.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns><see langword="true"/>, если соединение с БД устанавливается.</returns>
    public static Task<bool> CanConnectMedia(this IServiceProvider services, CancellationToken cancellationToken = default) =>
        services.GetRequiredService<MediaDbContext>().Database.CanConnectAsync(cancellationToken);
}
