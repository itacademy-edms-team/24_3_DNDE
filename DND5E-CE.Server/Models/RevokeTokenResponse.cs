namespace DND5E_CE.Server.Models
{
    public class RevokeTokenResponse
    {
        public bool Success { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
