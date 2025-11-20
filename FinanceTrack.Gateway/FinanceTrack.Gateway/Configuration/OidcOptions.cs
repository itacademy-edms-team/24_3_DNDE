namespace FinanceTrack.Gateway.Configuration
{
    public class OidcOptions
    {
        public const string SectionName = "Oidc";
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
