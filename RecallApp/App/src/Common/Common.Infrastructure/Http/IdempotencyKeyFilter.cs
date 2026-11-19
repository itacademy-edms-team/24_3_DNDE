using Common.Application.Idempotency;
using Microsoft.AspNetCore.Http;

namespace Common.Infrastructure.Http;

/// <summary>
/// Проверяет и нормализует заголовок Idempotency-Key на HTTP-слое.
/// </summary>
/// <remarks>
/// <para>Проверяются наличие и формат (UUID);
/// значение приводится к каноничному виду (ADR 0038: форма запроса проверяется на HTTP-слое).</para>
/// <para>Нормализация обязательна: в БД ключ хранится строкой, поэтому <c>{ABC...}</c>,
/// <c>ABC...</c> и <c>abc...</c> иначе стали бы разными ключами.</para>
/// </remarks>
public sealed class IdempotencyKeyFilter : IEndpointFilter
{
    /// <summary>Имя заголовка идемпотентности.</summary>
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

    /// <summary>
    /// Достаёт нормализованный ключ, который положил фильтр.
    /// </summary>
    /// <param name="httpContext">Контекст запроса.</param>
    /// <returns>Нормализованный ключ в каноничном виде UUID.</returns>
    /// <exception cref="InvalidOperationException">Фильтр не выполнился до чтения ключа.</exception>
    public static string GetCanonicalKey(HttpContext httpContext) =>
        httpContext.Items[CanonicalKeyItem] as string
        ?? throw new InvalidOperationException(
            $"{nameof(IdempotencyKeyFilter)} must run before the endpoint reads the idempotency key.");
}
