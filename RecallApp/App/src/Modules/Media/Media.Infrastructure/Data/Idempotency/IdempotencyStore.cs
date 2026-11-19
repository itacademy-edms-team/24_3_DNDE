using System.Text.Json;
using Common.Application.Idempotency;
using ErrorOr;
using Media.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Media.Infrastructure.Data.Idempotency;

/// <summary>
/// Реализует хранилище идемпотентности модуля Media (режим A).
/// </summary>
/// <remarks>
/// Ключ и бизнес-запись коммитятся в одной транзакции, поэтому состояния "в процессе" в БД
/// не остаётся. Подходит для коротких операций (создание строки загрузки);
/// подтверждение файла с сетью и CPU-операции использует режим B.
/// </remarks>
internal sealed class IdempotencyStore(MediaDbContext context, TimeProvider clock) : IMediaIdempotencyStore
{
    /// <summary>Время жизни ключа: повторы после суток считаются новым запросом.</summary>
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(24);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    
    public async Task<ErrorOr<TResponse>> ExecuteAsync<TResponse>(
        Guid ownerId,
        string key,
        string operation,
        string requestHash,
        Func<CancellationToken, Task<ErrorOr<TResponse>>> operationBody,
        CancellationToken cancellationToken)
        where TResponse : class
    {
        var now = clock.GetUtcNow().UtcDateTime;

        // Ambient-режим: если транзакция уже открыта вызывающим кодом, ключ и бизнес-запись
        // станут частью его транзакции, а коммит/откат останется на стороне начавшего транзакцию.
        var ownsTransaction = context.Database.CurrentTransaction is null;
        await using var transaction = ownsTransaction
            ? await context.Database.BeginTransactionAsync(cancellationToken)
            : null;

        var (reserved, existing) = await ReserveAsync(ownerId, key, operation, requestHash, now, cancellationToken);

        if (!reserved)
        {
            if (existing!.Operation != operation || existing.RequestHash != requestHash)
                return IdempotencyErrors.KeyReused(key);

            if (existing.StatusCode is null || existing.Response is null)
                return IdempotencyErrors.InProgress(key);

            return JsonSerializer.Deserialize<TResponse>(existing.Response, JsonOptions)
                ?? throw new InvalidOperationException(
                    $"Stored idempotent response for '{key}' is not a {typeof(TResponse).Name}.");
        }

        var result = await operationBody(cancellationToken);
        if (result.IsError)
            return result.Errors;

        await CompleteAsync(ownerId, key, statusCode: 201,
            JsonSerializer.Serialize(result.Value, JsonOptions), cancellationToken);

        if (ownsTransaction)
            await transaction!.CommitAsync(cancellationToken);

        return result;
    }

    private async Task<(bool Reserved, IdempotencyKeyRecord? Existing)> ReserveAsync(
        Guid ownerId,
        string key,
        string operation,
        string requestHash,
        DateTime now,
        CancellationToken cancellationToken)
    {
        // Две попытки: истёкший ключ удаляется, а между удалением и вставкой
        // может вклиниться конкурент или фоновый чистильщик.
        for (var attempt = 0; attempt < 2; attempt++)
        {
            if (await TryInsertAsync(ownerId, key, operation, requestHash, now, cancellationToken))
                return (true, null);

            var existing = await context.IdempotencyKeys
                .AsNoTracking()
                .SingleOrDefaultAsync(record => record.OwnerId == ownerId && record.Key == key, cancellationToken);

            if (existing is null)
                continue;

            if (existing.ExpiresAt > now)
                return (false, existing);

            // Истёкший ключ не переиспользуем: удаляем и вставляем новый.
            await context.IdempotencyKeys
                .Where(record => record.OwnerId == ownerId && record.Key == key && record.ExpiresAt <= now)
                .ExecuteDeleteAsync(cancellationToken);
        }

        throw new InvalidOperationException($"Could not reserve idempotency key '{key}' after retries.");
    }

    /// <summary>
    /// Вставка нового ключа. У EF Core нет upsert: ON CONFLICT DO NOTHING заодно сериализует конкурентов
    /// на уникальном индексе, а поймать 23505 через SaveChanges нельзя - ошибка абортит транзакцию Postgres.
    /// </summary>
    private async Task<bool> TryInsertAsync(
        Guid ownerId,
        string key,
        string operation,
        string requestHash,
        DateTime now,
        CancellationToken cancellationToken)
    {
        const string insertSql = """
            INSERT INTO media.idempotency_keys
                (owner_id, key, operation, request_hash, status_code, response, expires_at, created_at)
            VALUES (@owner_id, @key, @operation, @request_hash, NULL, NULL, @expires_at, @created_at)
            ON CONFLICT (owner_id, key) DO NOTHING
            """;

        var inserted = await context.Database.ExecuteSqlRawAsync(insertSql,
            new object[]
            {
                new NpgsqlParameter<Guid>("owner_id", ownerId),
                new NpgsqlParameter<string>("key", key),
                new NpgsqlParameter<string>("operation", operation),
                new NpgsqlParameter<string>("request_hash", requestHash),
                new NpgsqlParameter<DateTime>("expires_at", now.Add(Ttl)),
                new NpgsqlParameter<DateTime>("created_at", now),
            },
            cancellationToken);

        return inserted > 0;
    }

    private Task CompleteAsync(
        Guid ownerId,
        string key,
        int statusCode,
        string response,
        CancellationToken cancellationToken) =>
        context.IdempotencyKeys
            .Where(record => record.OwnerId == ownerId && record.Key == key)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(record => record.StatusCode, statusCode)
                .SetProperty(record => record.Response, response), cancellationToken);
}
