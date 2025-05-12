using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DND5E_CE.Server.Models;

namespace DND5E_CE.Server.Data
{
    public class DND5EContext: IdentityDbContext<IdentityUser>
    {
        public DND5EContext(DbContextOptions<DND5EContext> options)
            : base(options)
        {
        }

        public DbSet<RefreshTokenModel> RefreshTokens { get; init; } = null!;
        public DbSet<CsrfTokenModel> CsrfTokens { get; init; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure identity schema models here.
            modelBuilder.Entity<IdentityUser>().ToTable("Users", "identity");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles", "identity");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles", "identity");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims", "identity");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins", "identity");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims", "identity");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens", "identity");

            // Configure tokens schema models here.
            modelBuilder.Entity<RefreshTokenModel>().ToTable("RefreshTokens", "tokens");
            modelBuilder.Entity<RefreshTokenModel>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RefreshTokenModel>()
                .HasIndex(rt => rt.Token)
                .IsUnique();

            modelBuilder.Entity<CsrfTokenModel>()
                .ToTable("CsrfTokens", "tokens");
            modelBuilder.Entity<CsrfTokenModel>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CsrfTokenModel>()
                .HasIndex(t => new { t.Token, t.UserId })
                .IsUnique();

            // Configure public schema models here.
        }
    }
}
