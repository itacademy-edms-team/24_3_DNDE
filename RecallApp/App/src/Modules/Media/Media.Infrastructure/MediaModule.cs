using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Media.Infrastructure.Data;
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
