namespace Media.Contracts;

/// <summary>
/// Представляет ссылку на готовый вариант файла.
/// </summary>
/// <param name="Url">Подписанная ссылка с ограниченным сроком жизни.</param>
/// <param name="Width">Ширина варианта в пикселях.</param>
/// <param name="Height">Высота варианта в пикселях.</param>
public sealed record MediaImageLink(string Url, int Width, int Height);

/// <summary>
/// Представляет ссылки на варианты одного файла.
/// </summary>
/// <remarks>
/// <para>Ключи - имена вариантов (<c>thumb</c>, <c>display</c>);
/// какой вариант показать, решает клиент (онтология Media §10 п. 2).</para>
/// <para>Для <c>pending</c> и <c>failed</c> карта пуста.</para>
/// </remarks>
public sealed record MediaFileLinks(IReadOnlyDictionary<string, MediaImageLink> Variants);
