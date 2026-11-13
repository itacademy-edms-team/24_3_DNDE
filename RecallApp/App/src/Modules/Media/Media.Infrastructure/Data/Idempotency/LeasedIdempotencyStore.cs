using System.Text.Json;
using Common.Application.Idempotency;
using ErrorOr;
using Media.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Media.Infrastructure.Data.Idempotency;

/// <summary>
/// Журнал идемпотентности Media для долгих операций: хранит ключ со статусом выполнения и арендой,
/// отдаёт сохранённый результат при повторе и не даёт двум запросам обрабатывать один ключ одновременно.
/// Аренду продлевает, пока идёт работа: медленная обработка не выглядит зависшей,
/// а оборвавшаяся - освобождает ключ по истечении срока.
/// </summary>
internal sealed class LeasedIdempotencyStore(
    MediaDbContext context,
    NpgsqlDataSource dataSource,
    TimeProvider clock) : IUploadIdempotencyStore
{
    private const string InProgress = "in_progress";
    private const string Completed = "completed";

    /// <summary>Время жизни ключа: повторы после суток считаются новым запросом.</summary>
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(24);

    /// <summary>Срок аренды: столько времени обработчик считается живым без продления.</summary>
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromSeconds(60);

    /// <summary>Продление аренды: три продления на срок, чтобы сетевые задержки не рвали аренду.</summary>
    private static readonly TimeSpan RenewInterval = TimeSpan.FromSeconds(20);

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
        var leaseOwner = Guid.NewGuid();
        var now = clock.GetUtcNow().UtcDateTime;

        if (!await TryReserveAsync(ownerId, key, operation, requestHash, leaseOwner, now, cancellationToken))
        {
            var existing = await GetAsync(ownerId, key, cancellationToken);
            if (existing is null)
                return IdempotencyErrors.InProgress(key);

            if (existing.Operation != operation || existing.RequestHash != requestHash)
                return IdempotencyErrors.KeyReused(key);

            // Готовый ключ: повтор отдаёт сохранённый результат и не выполняет работу.
            if (existing.Response is not null)
                return JsonSerializer.Deserialize<TResponse>(existing.Response, JsonOptions)
                    ?? throw new InvalidOperationException(
                        $"Stored idempotent response for '{key}' is not a {typeof(TResponse).Name}.");

            // Живая аренда: второй запрос ждать не должен, он получает конфликт.
            if (existing.LeaseUntil is not null && existing.LeaseUntil > now)
                return IdempotencyErrors.InProgress(key);

            // Аренда истекла: предыдущий обработчик не завершил работу, поэтому выполняем её заново.
            if (!await TryTakeOverAsync(ownerId, key, leaseOwner, now, cancellationToken))
                return IdempotencyErrors.InProgress(key);
        }

        using var renewalCancellation = new CancellationTokenSource();
        var renewal = RenewLoopAsync(ownerId, key, leaseOwner, renewalCancellation.Token);

        ErrorOr<TResponse> result;
        try
        {
            result = await operationBody(cancellationToken);
        }
        finally
        {
            renewalCancellation.Cancel();
            try
            {
                await renewal;
            }
            catch (OperationCanceledException)
            {
                // Продление остановлено штатно: работа уже завершилась.
            }
        }

        if (result.IsError)
        {
            // Неудачу не кешируем: клиент должен иметь возможность повторить.
            await ReleaseAsync(ownerId, key, leaseOwner, cancellationToken);
            return result.Errors;
        }

        await CompleteAsync(ownerId, key, leaseOwner,
            JsonSerializer.Serialize(result.Value, JsonOptions), cancellationToken);

        return result;
    }

    private async Task<bool> TryReserveAsync(
        Guid ownerId,
        string key,
        string operation,
        string requestHash,
        Guid leaseOwner,
        DateTime now,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO media.idempotency_keys
                (owner_id, key, operation, request_hash, status_code, response, status, lease_owner, lease_until, expires_at, created_at)
            VALUES (@owner_id, @key, @operation, @request_hash, NULL, NULL, @status, @lease_owner, @lease_until, @expires_at, @created_at)
            ON CONFLICT (owner_id, key) DO NOTHING
            """;

        var inserted = await context.Database.ExecuteSqlRawAsync(sql,
            new object[]
            {
                new NpgsqlParameter<Guid>("owner_id", ownerId),
                new NpgsqlParameter<string>("key", key),
                new NpgsqlParameter<string>("operation", operation),
                new NpgsqlParameter<string>("request_hash", requestHash),
                new NpgsqlParameter<string>("status", InProgress),
                new NpgsqlParameter<Guid>("lease_owner", leaseOwner),
                new NpgsqlParameter<DateTime>("lease_until", now.Add(LeaseDuration)),
                new NpgsqlParameter<DateTime>("expires_at", now.Add(Ttl)),
                new NpgsqlParameter<DateTime>("created_at", now),
            },
            cancellationToken);

        return inserted > 0;
    }

    private Task<IdempotencyKeyRecord?> GetAsync(Guid ownerId, string key, CancellationToken cancellationToken) =>
        context.IdempotencyKeys
            .AsNoTracking()
            .SingleOrDefaultAsync(record => record.OwnerId == ownerId && record.Key == key, cancellationToken);

    private async Task<bool> TryTakeOverAsync(
        Guid ownerId,
        string key,
        Guid leaseOwner,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var leaseUntil = now.Add(LeaseDuration);

        var updated = await context.IdempotencyKeys
            .Where(record => record.OwnerId == ownerId 
                             && record.Key == key 
                             && (record.LeaseUntil == null || record.LeaseUntil <= now))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(record => record.LeaseOwner, leaseOwner)
                .SetProperty(record => record.LeaseUntil, leaseUntil),
                cancellationToken);

        return updated > 0;
    }

    private async Task RenewLoopAsync(Guid ownerId, string key, Guid leaseOwner, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE media.idempotency_keys
            SET lease_until = @lease_until
            WHERE owner_id = @owner_id AND key = @key AND lease_owner = @lease_owner AND status = @status
            """;

        // Своё соединение: фоновое продление не должно делить DbContext с бизнес-операцией,
        // иначе тик таймера и SaveChanges могут пересечься (DbContext не потокобезопасен).
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        using var timer = new PeriodicTimer(RenewInterval);
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add(new NpgsqlParameter<DateTime>("lease_until", clock.GetUtcNow().UtcDateTime.Add(LeaseDuration)));
            command.Parameters.Add(new NpgsqlParameter<Guid>("owner_id", ownerId));
            command.Parameters.Add(new NpgsqlParameter<string>("key", key));
            command.Parameters.Add(new NpgsqlParameter<Guid>("lease_owner", leaseOwner));
            command.Parameters.Add(new NpgsqlParameter<string>("status", InProgress));
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Записывает результат выполненной операции в журнал и снимает аренду.
    /// Срабатывает только для своей аренды.
    /// </summary>
    private async Task CompleteAsync(
        Guid ownerId,
        string key,
        Guid leaseOwner,
        string response,
        CancellationToken cancellationToken)
    {
        // Финализация raw-SQL: свойство <c>Response</c> - строка, а колонка `jsonb`,
        // поэтому нужен каст `@response::jsonb`, которого не выразить в <c>ExecuteUpdate</c>.
        const string sql = """
            UPDATE media.idempotency_keys
            SET status = @completed, status_code = 201, response = @response::jsonb,
                lease_owner = NULL, lease_until = NULL
            WHERE owner_id = @owner_id AND key = @key AND lease_owner = @lease_owner AND status = @in_progress
            """;

        await context.Database.ExecuteSqlRawAsync(sql,
            new object[]
            {
                new NpgsqlParameter<Guid>("owner_id", ownerId),
                new NpgsqlParameter<string>("key", key),
                new NpgsqlParameter<Guid>("lease_owner", leaseOwner),
                new NpgsqlParameter<string>("response", response),
                new NpgsqlParameter<string>("completed", Completed),
                new NpgsqlParameter<string>("in_progress", InProgress),
            },
            cancellationToken);
    }
    
    private Task ReleaseAsync(Guid ownerId, string key, Guid leaseOwner, CancellationToken cancellationToken) =>
        context.IdempotencyKeys
            .Where(record => record.OwnerId == ownerId
                             && record.Key == key
                             && record.LeaseOwner == leaseOwner
                             && record.Status == InProgress)
            .ExecuteDeleteAsync(cancellationToken);
}
