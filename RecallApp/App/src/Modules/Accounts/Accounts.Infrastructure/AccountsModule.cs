using Accounts.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Accounts.Infrastructure;

/// <summary>
/// Композиция модуля Accounts: регистрация сервисов, маршруты, миграции, проверка связности.
/// Единственная публичная точка модуля; остальное - internal.
/// </summary>
public static class AccountsModule
{
    public static IServiceCollection AddAccounts(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AccountsDbContext>(options => options.UseNpgsql(connectionString,
            pg => pg.MigrationsHistoryTable("__EFMigrationsHistory", "accounts")));

        return services;
    }

    public static IEndpointRouteBuilder MapAccounts(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/users").WithTags("Accounts");

        group.MapGet("/me", (HttpContext context) =>
        {
            var sub = context.User.FindFirst("sub")?.Value;
            var email = context.User.FindFirst("email")?.Value;
            var name = context.User.FindFirst("name")?.Value;
            var emailVerified = context.User.FindFirst("email_verified")?.Value == "true";
            return Results.Ok(new { sub, email, name, emailVerified });
        }).RequireAuthorization();

        return endpoints;
    }

    /// <summary>
    /// Применяет миграции модуля.
    /// </summary>
    /// <param name="connectionString">Строка подключения к БД пользователя, привязанного к модулю и схеме Accounts.</param>
    /// <param name="cancellationToken">Токен отслеживания отмены операции.</param>
    public static async Task MigrateAccounts(string connectionString, CancellationToken cancellationToken = default)
    {
        var options = new DbContextOptionsBuilder<AccountsDbContext>().UseNpgsql(connectionString,
            pg => pg.MigrationsHistoryTable("__EFMigrationsHistory", "accounts")).Options;
        await using var db = new AccountsDbContext(options);
        await db.Database.MigrateAsync(cancellationToken);
    }

    public static Task<bool> CanConnectAccounts(this IServiceProvider services, CancellationToken cancellationToken = default) =>
        services.GetRequiredService<AccountsDbContext>().Database.CanConnectAsync(cancellationToken);
}
