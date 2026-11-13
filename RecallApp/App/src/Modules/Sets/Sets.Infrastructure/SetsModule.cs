using Common.Application.Behaviors;
using Common.Application.Idempotency;
using Common.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Sets.Application.Abstractions;
using Sets.Application.Elements;
using Sets.Application.Sets;
using Sets.Infrastructure.Data;
using Sets.Infrastructure.Data.Idempotency;
using Sets.Infrastructure.Data.Purge;

namespace Sets.Infrastructure;

/// <summary>
/// Композиция модуля Sets: сервисы, маршруты, миграции, проверка связности.
/// Единственная публичная точка модуля.
/// </summary>
public static class SetsModule
{
    /// <summary>Регистрирует модуль на общем runtime-пуле соединений (квота выдаётся хостом).</summary>
    public static IServiceCollection AddSets(this IServiceCollection services, NpgsqlDataSource dataSource)
    {
        services.AddDbContext<SetsDbContext>(options => options.UseNpgsql(dataSource,
            pg => pg.MigrationsHistoryTable("__EFMigrationsHistory", "sets")));

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(ISetRepository).Assembly);
            configuration.AddCommandValidation();
        });
        services.AddCommandValidators(typeof(ISetRepository));
        services.AddScoped<ISetRepository, SetRepository>();
        services.AddScoped<IElementRepository, ElementRepository>();
        services.AddScoped<ISetsIdempotencyStore, IdempotencyStore>();

        // Фоновая чистка истёкших ключей.
        services.AddOptions<IdempotencyCleanupOptions>().BindConfiguration("Sets:Idempotency");
        services.AddHostedService<IdempotencyCleanupService>();

        // Физическая чистка мягко удалённого контента после окна retention (ADR 0036).
        services.AddOptions<SetsPurgeOptions>().BindConfiguration("Sets:Purge");
        services.AddHostedService<SetsPurgeService>();

        return services;
    }

    public static IEndpointRouteBuilder MapSets(this IEndpointRouteBuilder endpoints)
    {
        MapSetEndpoints(endpoints);
        MapElementEndpoints(endpoints);
        return endpoints;
    }

    private static void MapSetEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(SetRoute.Group).WithTags("Sets").RequireAuthorization();

        group.MapGet("/", async (string? cursor, int? pageSize, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new ListSetsQuery(cursor, pageSize), cancellationToken);
            return result.IsError ? result.Errors.ToProblem() : Results.Ok(result.Value);
        });

        // Набор без элементов: элементы приходят отдельным запросом.
        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetSetQuery(id), cancellationToken);
            return result.IsError ? result.Errors.ToProblem() : Results.Ok(result.Value);
        });

        group.MapPost("/", async (CreateSetRequest request, HttpContext http, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new CreateSetCommand(request.Title, request.Description, IdempotencyKeyFilter.GetCanonicalKey(http)),
                cancellationToken);

            return result.IsError
                ? result.Errors.ToProblem()
                : Results.Created($"{SetRoute.Group}/{result.Value.Id}", result.Value);
        }).AddEndpointFilter<IdempotencyKeyFilter>();

        group.MapPut("/{id:guid}", async (Guid id, UpdateSetRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new UpdateSetCommand(id, request.Title, request.Description), cancellationToken);
            return result.IsError ? result.Errors.ToProblem() : Results.Ok(result.Value);
        });

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeleteSetCommand(id), cancellationToken);
            return result.IsError ? result.Errors.ToProblem() : Results.NoContent();
        });
    }

    private static void MapElementEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ElementRoute.Group).WithTags("Elements").RequireAuthorization();

        group.MapGet("/", async (Guid setId, string? cursor, int? pageSize, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new ListElementsQuery(setId, cursor, pageSize), cancellationToken);
            return result.IsError ? result.Errors.ToProblem() : Results.Ok(result.Value);
        });

        group.MapPost("/", async (Guid setId, CreateElementRequest request, HttpContext http, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new CreateElementCommand(setId, request.Type, request.Payload, request.Position,
                    IdempotencyKeyFilter.GetCanonicalKey(http)),
                cancellationToken);

            return result.IsError
                ? result.Errors.ToProblem()
                : Results.Created($"{SetRoute.Group}/{setId}/elements/{result.Value.Id}", result.Value);
        }).AddEndpointFilter<IdempotencyKeyFilter>();

        group.MapPut("/{elementId:guid}", async (
            Guid setId,
            Guid elementId,
            UpdateElementRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateElementCommand(setId, elementId, request.Payload),
                cancellationToken);

            return result.IsError ? result.Errors.ToProblem() : Results.Ok(result.Value);
        });

        group.MapDelete("/{elementId:guid}", async (Guid setId, Guid elementId, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeleteElementCommand(setId, elementId), cancellationToken);
            return result.IsError ? result.Errors.ToProblem() : Results.NoContent();
        });

        // Полная перестановка: маршрут с фиксированным сегментом не конфликтует с {elementId:guid}.
        group.MapPut("/order", async (Guid setId, ReorderElementsRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new ReorderElementsCommand(setId, request.ElementIds), cancellationToken);
            return result.IsError ? result.Errors.ToProblem() : Results.NoContent();
        });
    }

    /// <summary>Применяет миграции модуля.</summary>
    /// <param name="connectionString">Строка подключения пользователя, владеющего схемой sets.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public static async Task MigrateSets(string connectionString, CancellationToken cancellationToken = default)
    {
        var options = new DbContextOptionsBuilder<SetsDbContext>().UseNpgsql(connectionString,
            pg => pg.MigrationsHistoryTable("__EFMigrationsHistory", "sets")).Options;
        await using var dbContext = new SetsDbContext(options);
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    public static Task<bool> CanConnectSets(this IServiceProvider services, CancellationToken cancellationToken = default) =>
        services.GetRequiredService<SetsDbContext>().Database.CanConnectAsync(cancellationToken);
}
