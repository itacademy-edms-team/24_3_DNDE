using Accounts.Application.Abstractions;
using Accounts.Application.Users;
using Accounts.Infrastructure.Data;
using Common.Application.Provisioning;
using Common.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Accounts.Infrastructure;

/// <summary>
/// Предоставляет композицию модуля Accounts:
/// регистрацию сервисов, маршруты, миграции и проверку связности.
/// </summary>
/// <remarks>
/// Единственная публичная точка модуля: всё остальное под <c>internal</c>.
/// </remarks>
public static class AccountsModule
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
    public static IServiceCollection AddAccounts(this IServiceCollection services, NpgsqlDataSource dataSource)
    {
        services.AddDbContext<AccountsDbContext>(options => options.UseNpgsql(dataSource,
            pg => pg.MigrationsHistoryTable("__EFMigrationsHistory", "accounts")));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IUserRepository).Assembly));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDomainProvisioner, UserProvisioner>();

        return services;
    }

    /// <summary>
    /// Регистрирует маршруты модуля.
    /// </summary>
    /// <param name="endpoints">Построитель маршрутов хоста.</param>
    /// <returns>Тот же построитель для цепочки.</returns>
    public static IEndpointRouteBuilder MapAccounts(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/users").WithTags("Accounts");

        group.MapGet("/me", async (HttpContext context, ISender sender, CancellationToken cancellationToken) =>
        {
            var emailVerified = context.User.FindFirst("email_verified")?.Value == "true";
            var result = await sender.Send(
                new GetMyAccountQuery(context.User.FindFirst("sub")?.Value, emailVerified),
                cancellationToken);

            // Ошибки домена отдаются ProblemDetails (ADR 0031): код в title, source добавляет хост.
            return result.IsError ? result.Errors.ToProblem() : Results.Ok(result.Value);
        }).RequireAuthorization();

        return endpoints;
    }

    /// <summary>
    /// Применяет миграции модуля.
    /// </summary>
    /// <param name="connectionString">Строка подключения пользователя-владельца схемы accounts.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public static async Task MigrateAccounts(string connectionString, CancellationToken cancellationToken = default)
    {
        var options = new DbContextOptionsBuilder<AccountsDbContext>().UseNpgsql(connectionString,
            pg => pg.MigrationsHistoryTable("__EFMigrationsHistory", "accounts")).Options;
        await using var db = new AccountsDbContext(options);
        await db.Database.MigrateAsync(cancellationToken);
    }

    /// <summary>
    /// Проверяет, доступна ли схема модуля.
    /// </summary>
    /// <param name="services">Провайдер сервисов хоста.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns><see langword="true"/>, если соединение с БД устанавливается.</returns>
    public static Task<bool> CanConnectAccounts(this IServiceProvider services, CancellationToken cancellationToken = default) =>
        services.GetRequiredService<AccountsDbContext>().Database.CanConnectAsync(cancellationToken);
}
