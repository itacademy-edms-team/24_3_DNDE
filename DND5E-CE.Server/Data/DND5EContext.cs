using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DND5E_CE.Server.Models;
using DND5E_CE.Server.Models.App;

namespace DND5E_CE.Server.Data
{
    public class DND5EContext: IdentityDbContext<IdentityUser>
    {
        public DND5EContext(DbContextOptions<DND5EContext> options)
            : base(options)
        {
        }

        public DbSet<RefreshTokenModel> RefreshTokens { get; set; }
        public DbSet<CsrfTokenModel> CsrfTokens { get; set; }
        public DbSet<Sheet1Model> Sheet1 { get; set; }
        public DbSet<Sheet2Model> Sheet2 { get; set; }
        public DbSet<Sheet3Model> Sheet3 { get; set; }
        public DbSet<CharacterModel> Character { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure identity schema models here.
            modelBuilder.Entity<IdentityUser>()
                .ToTable("Users", "identity");
            modelBuilder.Entity<IdentityRole>()
                .ToTable("Roles", "identity");
            modelBuilder.Entity<IdentityUserRole<string>>()
                .ToTable("UserRoles", "identity");
            modelBuilder.Entity<IdentityUserClaim<string>>()
                .ToTable("UserClaims", "identity");
            modelBuilder.Entity<IdentityUserLogin<string>>()
                .ToTable("UserLogins", "identity");
            modelBuilder.Entity<IdentityRoleClaim<string>>()
                .ToTable("RoleClaims", "identity");
            modelBuilder.Entity<IdentityUserToken<string>>()
                .ToTable("UserTokens", "identity");

            // Configure tokens schema models here.
            modelBuilder.Entity<RefreshTokenModel>(action =>
            {
                action.ToTable("RefreshTokens", "tokens");

                action.HasOne(rt => rt.User)
                    .WithMany()
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                action.HasIndex(rt => rt.Token)
                    .IsUnique();
            });

            modelBuilder.Entity<CsrfTokenModel>(action =>
            {
                action.ToTable("CsrfTokens", "tokens");

                action.HasOne(t => t.User)
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                action.HasIndex(t => new { t.Token, t.UserId })
                    .IsUnique();
            });

            // Configure app schema models here.
            modelBuilder.Entity<Sheet1Model>(action =>
            {
                action.ToTable("Sheet1Models", "app");

                action.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                action.HasIndex(s => s.UserId);
            });

            modelBuilder.Entity<Sheet2Model>(action =>
            {
                action.ToTable("Sheet2Models", "app");

                action.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                action.HasIndex(s => s.UserId);
            });

            modelBuilder.Entity<Sheet3Model>(action =>
            {
                action.ToTable("Sheet3Models", "app");

                action.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                action.HasIndex(s => s.UserId);
            });

            modelBuilder.Entity<CharacterModel>(action =>
            {
                action.ToTable("CharacterModels", "app");

                action.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                action.HasIndex(c => c.UserId);
            });

            // Configure public schema models here.
        }
    }
}
