using Host.Configuration;
using Microsoft.Extensions.Options;

namespace Tests.Unit.Helpers;

/// <summary>Настройки SPA для тестов.</summary>
internal static class TestSpaOptions
{
    public const string BaseUrl = "https://spa.test";

    public static SpaOptions Create() => new() { BaseUrl = BaseUrl };

    public static IOptions<SpaOptions> Wrap(SpaOptions? options = null) =>
        Options.Create(options ?? Create());
}
