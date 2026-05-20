namespace FinanceTrack.Finance.Infrastructure.Ai;

public sealed class YandexAiOptions
{
    public const string SectionName = "YandexAi";

    public bool Enabled { get; set; } = false;
    public string ApiKey { get; set; } = string.Empty;
    public string FolderId { get; set; } = string.Empty;

    /// <summary>
    /// Short model name, e.g. "yandexgpt-lite" or "yandexgpt".
    /// Full URI gpt://{FolderId}/{Model}/latest is constructed at runtime.
    /// </summary>
    public string Model { get; set; } = "yandexgpt-lite";
}
