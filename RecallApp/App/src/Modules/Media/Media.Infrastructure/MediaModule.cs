using Amazon.S3;
using Common.Application.Behaviors;
using Common.Infrastructure.Http;
using MediatR;
using Media.Application.Abstractions;
using Media.Application.Images;
using Media.Application.Objects;
using Media.Application.Storage;
using Media.Infrastructure.Data;
using Media.Infrastructure.Data.Idempotency;
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
/// Композиция модуля Media: сервисы, миграции, проверка связности.
/// Единственная публичная точка модуля наряду с контрактом.
/// </summary>
public static class MediaModule
{
    /// <summary>Регистрирует модуль на общем runtime-пуле соединений (квота выдаётся хостом).</summary>
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
        services.AddSingleton<IImageProcessor, SkiaImageProcessor>();

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

    /// <summary>Регистрирует маршруты модуля.</summary>
    public static IEndpointRouteBuilder MapMedia(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(MediaRoute.Group).WithTags("Media").RequireAuthorization();

        group.MapPost("/uploads", async (InitUploadRequest request, HttpContext http, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new InitUploadCommand(request.ContentType, request.SizeBytes,
                    IdempotencyKeyFilter.GetCanonicalKey(http)),
                cancellationToken);

            return result.IsError
                ? result.Errors.ToProblem()
                : Results.Created($"{MediaRoute.Group}/{result.Value.MediaId}", result.Value);
        }).AddEndpointFilter<IdempotencyKeyFilter>();

        return endpoints;
    }

    /// <summary>Применяет миграции модуля.</summary>
    /// <param name="connectionString">Строка подключения пользователя, владеющего схемой media.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public static async Task MigrateMedia(string connectionString, CancellationToken cancellationToken = default)
    {
        var options = new DbContextOptionsBuilder<MediaDbContext>().UseNpgsql(connectionString,
            pg => pg.MigrationsHistoryTable("__EFMigrationsHistory", "media")).Options;
        await using var dbContext = new MediaDbContext(options);
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    public static Task<bool> CanConnectMedia(this IServiceProvider services, CancellationToken cancellationToken = default) =>
        services.GetRequiredService<MediaDbContext>().Database.CanConnectAsync(cancellationToken);
}
