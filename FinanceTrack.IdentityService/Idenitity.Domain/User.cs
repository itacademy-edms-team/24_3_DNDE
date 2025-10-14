using Microsoft.AspNetCore.Identity;

namespace Identity.Domain
{
    public class User : IdentityUser<Guid>
    {
        public User() { }

        public User(string userName)
            : base(userName) { }
    }
}
