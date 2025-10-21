namespace Idenitity.Infrastructure.Services.Jwt
{
    public class JwtOptions
    {
        public AccessTokenOptions AccessTokenOptions { get; set; }
        public RefreshTokenOptions RefreshTokenOptions { get; set; }
    }

    public class AccessTokenOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public long LifeTimeInMinutes { get; set; }
        public string SigningKey { get; set; }
    }

    public class RefreshTokenOptions
    {
        public int LifeTimeInMinutes { get; set; }
    }
}
