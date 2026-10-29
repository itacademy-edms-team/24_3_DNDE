namespace Host.Services;

/// <summary>Результат обращения к token-endpoint. <see cref="Tokens"/> не null только при Success.</summary>
public sealed record TokenRefreshResult(TokenRefreshOutcome Outcome, RefreshedTokens? Tokens = null);
