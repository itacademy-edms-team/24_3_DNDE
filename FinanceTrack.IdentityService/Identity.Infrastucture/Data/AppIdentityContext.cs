using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastucture.Data
{
    public class AppIdentityContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public AppIdentityContext(DbContextOptions<AppIdentityContext> options)
            : base(options) { }
    }
}
