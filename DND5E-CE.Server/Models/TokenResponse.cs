namespace DND5E_CE.Server.Models
{
    public class TokenResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public bool Success { get; set; }
        public IReadOnlyList<string> Errors { get; init; }
    }
}
