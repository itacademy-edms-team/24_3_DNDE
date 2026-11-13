namespace Media.Contracts;

/// <summary>Ссылка на готовый вариант файла: адрес для чтения и фактические размеры.</summary>
/// <param name="Url">Подписанная ссылка с ограниченным сроком жизни.</param>
/// <param name="Width">Ширина варианта в пикселях.</param>
/// <param name="Height">Высота варианта в пикселях.</param>
public sealed record MediaImageLink(string Url, int Width, int Height);

/// <summary>
/// Ссылки на варианты одного файла: клиент выбирает размер сам (онтология Media §10 п. 2).
/// Ключи — имена вариантов (`thumb`, `display`); в `pending`/`failed` карта пуста.
/// </summary>
public sealed record MediaFileLinks(IReadOnlyDictionary<string, MediaImageLink> Variants);
