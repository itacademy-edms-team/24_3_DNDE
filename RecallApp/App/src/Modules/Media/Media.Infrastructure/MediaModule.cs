using Amazon.S3;
using MediatR;
using Media.Application.Storage;
using Media.Infrastructure.Data;
using Media.Infrastructure.Storage;
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
            configuration.RegisterServicesFromAssembly(typeof(MediaModule).Assembly));

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
