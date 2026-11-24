using System.Text.Json;
using Common.Application.Idempotency;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Sets.Application.Abstractions;

namespace Sets.Infrastructure.Data.Idempotency;

/// <summary>
/// Обеспечивает идемпотентность запросов в схеме sets.
/// </summary>
/// <remarks>
/// <para>Ключ и бизнес-запись коммитятся вместе, поэтому состояния «в процессе» в базе не остаётся:
/// либо есть сохранённый ответ, либо ключа нет.</para>
/// <para>Если вызывающий код уже открыл транзакцию (например, под блокировкой набора),
/// хранилище встраивается в неё и не управляет коммитом - это делает владелец транзакции.</para>
/// </remarks>
internal sealed class IdempotencyStore(SetsDbContext context, TimeProvider clock) : ISetsIdempotencyStore
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

            // В норме сюда не попадаем: незавершённый ключ не может закоммититься.
            if (existing.StatusCode is null || existing.Response is null)
                return IdempotencyErrors.InProgress(key);

            return JsonSerializer.Deserialize<TResponse>(existing.Response, JsonOptions)
                ?? throw new InvalidOperationException($"Stored idempotent response for '{key}' is not a {typeof(TResponse).Name}.");
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

    /// <summary>
    /// Резервирует ключ.
    /// </summary>
    /// <returns>
    /// (true, null) - ключ наш, операцию нужно выполнить; (false, existing) - ключ уже использован, надо отдать сохранённый ответ или конфликт.
    /// </returns>
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
    /// Вставка нового ключа. Единственный raw-запрос: у EF Core нет upsert.
    /// Поймать 23505 через SaveChanges нельзя - ошибка абортит транзакцию Postgres, и продолжить
    /// в ней уже не получится. ON CONFLICT DO NOTHING заодно сериализует конкурентов:
    /// второй ждёт наш COMMIT на уникальном индексе.
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
            INSERT INTO sets.idempotency_keys
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

    /// <summary>
    /// Записывает ответ и статус в зарезервированный ключ.
    /// </summary>
    /// <param name="ownerId">Владелец ключа.</param>
    /// <param name="key">Ключ идемпотентности.</param>
    /// <param name="statusCode">HTTP-статус сохранённого ответа.</param>
    /// <param name="responseJson">Сериализованный ответ.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    private Task<int> CompleteAsync(
        Guid ownerId,
        string key,
        int statusCode,
        string responseJson,
        CancellationToken cancellationToken) =>
        context.IdempotencyKeys
            .Where(record => record.OwnerId == ownerId && record.Key == key)
            .ExecuteUpdateAsync(update => update
                .SetProperty(record => record.StatusCode, statusCode)
                .SetProperty(record => record.Response, responseJson), cancellationToken);
}
