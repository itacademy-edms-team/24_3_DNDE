namespace Common.Application.Outbox;

/// <summary>
/// Полосы доставки outbox (ADR 0040).
/// </summary>
public static class OutboxLanes
{
    public const string Fast = "fast";

    public const string Slow = "slow";
}
