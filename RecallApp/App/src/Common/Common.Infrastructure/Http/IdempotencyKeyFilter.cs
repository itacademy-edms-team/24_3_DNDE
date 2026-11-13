using Common.Application.Idempotency;
using Microsoft.AspNetCore.Http;

namespace Common.Infrastructure.Http;

/// <summary>
/// Транспортная проверка обязательного заголовка `Idempotency-Key` (ADR 0038: форма запроса - на HTTP-слое).
/// Фильтр проверяет наличие и формат, а также нормализует значение к каноничному виду UUID:
/// в БД ключ хранится строкой, поэтому «{ABC...}», «ABC...» и «abc...» иначе стали бы разными ключами.
/// </summary>
public sealed class IdempotencyKeyFilter : IEndpointFilter
{
    public const string HeaderName = "Idempotency-Key";

    /// <summary>Ключ в <see cref="HttpContext.Items"/> с уже нормализованным значением.</summary>
    public const string CanonicalKeyItem = "Idempotency-Key.Canonical";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var header = context.HttpContext.Request.Headers[HeaderName].ToString();

        if (string.IsNullOrWhiteSpace(header))
            return IdempotencyErrors.KeyRequired.ToProblem();

        if (!Guid.TryParse(header, out var parsed))
            return IdempotencyErrors.KeyInvalid.ToProblem();

        context.HttpContext.Items[CanonicalKeyItem] = parsed.ToString();
        return await next(context);
    }

    /// <summary>Достаёт нормализованный ключ, который положил фильтр.</summary>
    public static string GetCanonicalKey(HttpContext httpContext) =>
        httpContext.Items[CanonicalKeyItem] as string
        ?? throw new InvalidOperationException(
            $"{nameof(IdempotencyKeyFilter)} must run before the endpoint reads the idempotency key.");
}
