namespace Identity.Application.Commands.RefreshToken.Request
{
    public class RefreshTokenRequest : Commands.Request
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
