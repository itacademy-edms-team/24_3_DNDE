namespace FinanceTrack.Gateway.Configuration
{
    public class BffOptions
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class OidcOptions
    {
        public const string SectionName = "Oidc";

        public BffOptions Bff { get; set; } = new();
    }
}
