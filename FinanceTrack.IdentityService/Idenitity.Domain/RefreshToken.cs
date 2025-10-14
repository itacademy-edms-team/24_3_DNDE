using System;

namespace Identity.Domain
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public bool IsRevoked { get; set; }
        public User User { get; set; }
    }
}
