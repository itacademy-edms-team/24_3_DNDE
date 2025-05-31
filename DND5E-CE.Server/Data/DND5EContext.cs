using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DND5E_CE.Server.Models;
using DND5E_CE.Server.Models.App;
using DND5E_CE.Server.Models.App.Sheet1;

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
        public DbSet<CharacterModel> Character { get; set; }
        public DbSet<Sheet1Model> Sheet1 { get; set; }

        public DbSet<AbilityModel> Ability { get; set; }
        public DbSet<AbilitySaveThrowModel> AbilitySaveThrow { get; set; }
        public DbSet<SkillModel> Skill { get; set; }
        public DbSet<ToolModel> Tool {  get; set; }
        public DbSet<OtherToolModel> OtherTool { get; set; }
        public DbSet<HitPointModel> HitPoint { get; set; }
        public DbSet<HitDiceModel> HitDice { get; set; }
        public DbSet<DeathSaveThrowModel> DeathSaveThrow {  get; set; }
        public DbSet<AttackModel> Attack {  get; set; }
        public DbSet<GlobalDamageModifierModel> GlobalDamageModifier { get; set; }
        public DbSet<InventoryGoldModel> InventoryGold { get; set; }
        public DbSet<InventoryItemModel> InventoryItem { get; set; }
        public DbSet<ClassResourceModel> ClassResource { get; set; }
        public DbSet<OtherResourceModel> OtherResource { get; set; }
        public DbSet<Sheet2Model> Sheet2 { get; set; }
        public DbSet<Sheet3Model> Sheet3 { get; set; }


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

            // Configure app schema
            modelBuilder.Entity<CharacterModel>(action =>
            {
                action.ToTable("Characters", "app");

                // Sheet1: one-to-one
                action.HasOne(c => c.Sheet1)
                    .WithOne(s => s.Character)
                    .HasForeignKey<CharacterModel>(c => c.Sheet1Id)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // Sheet2: one-to-one
                action.HasOne(c => c.Sheet2)
                    .WithOne(s => s.Character)
                    .HasForeignKey<CharacterModel>(c => c.Sheet2Id)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // Sheet3: one-to-one
                action.HasOne(c => c.Sheet3)
                    .WithOne(s => s.Character)
                    .HasForeignKey<CharacterModel>(c => c.Sheet3Id)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // User: one-to-one
                action.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                action.HasIndex(c => c.UserId);
            });

            modelBuilder.Entity<Sheet1Model>(action =>
            {
                action.ToTable("Sheet1", "app");

                // Ability: one-to-one
                action.HasOne(s => s.Ability)
                    .WithOne(a => a.Sheet1)
                    .HasForeignKey<Sheet1Model>(s => s.AbilityId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // AbilitySaveThrow: one-to-one
                action.HasOne(s => s.AbilitySaveThrow)
                    .WithOne(ast => ast.Sheet1)
                    .HasForeignKey<Sheet1Model>(s => s.AbilitySaveThrowId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // Skill: one-to-one
                action.HasOne(s => s.Skill)
                    .WithOne(sk => sk.Sheet1)
                    .HasForeignKey<Sheet1Model>(s => s.SkillId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // Tool: one-to-many
                action.HasMany(s => s.Tool)
                    .WithOne(t => t.Sheet1)
                    .HasForeignKey(t => t.Sheet1Id)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // OtherTool: one-to-many
                action.HasMany(s => s.OtherTool)
                    .WithOne(ot => ot.Sheet1)
                    .HasForeignKey(ot => ot.Sheet1Id)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // HitPoint: one-to-one
                action.HasOne(s => s.HitPoint)
                    .WithOne(hp => hp.Sheet1)
                    .HasForeignKey<Sheet1Model>(s => s.HitPointId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // HitDice: one-to-one
                action.HasOne(s => s.HitDice)
                    .WithOne(hd => hd.Sheet1)
                    .HasForeignKey<Sheet1Model>(s => s.HitDiceId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // DeathSaveThrow: one-to-one
                action.HasOne(s => s.DeathSaveThrow)
                    .WithOne(dst => dst.Sheet1)
                    .HasForeignKey<Sheet1Model>(s => s.DeathSaveThrowId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // Attack: one-to-many
                action.HasMany(s => s.Attack)
                    .WithOne(a => a.Sheet1)
                    .HasForeignKey(a => a.Sheet1Id)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // GlobalDamageModifier: one-to-many
                action.HasMany(s => s.GlobalDamageModifier)
                    .WithOne(g => g.Sheet1)
                    .HasForeignKey(g => g.Sheet1Id)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // InventoryGold: one-to-one
                action.HasOne(s => s.InventoryGold)
                    .WithOne(ig => ig.Sheet1)
                    .HasForeignKey<Sheet1Model>(s => s.InventoryGoldId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // InventoryItem: one-to-many
                action.HasMany(s => s.InventoryItem)
                    .WithOne(ii => ii.Sheet1)
                    .HasForeignKey(ii => ii.Sheet1Id)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // ClassResource: one-to-one
                action.HasOne(s => s.ClassResource)
                    .WithOne(cr => cr.Sheet1)
                    .HasForeignKey<Sheet1Model>(s => s.ClassResourceId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // OtherResource: one-to-many
                action.HasMany(s => s.OtherResource)
                    .WithOne(ii => ii.Sheet1)
                    .HasForeignKey(ii => ii.Sheet1Id)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity<AbilityModel>(action =>
            {
                action.ToTable("Abilities", "app");
            });

            modelBuilder.Entity<SkillModel>(action =>
            {
                action.ToTable("Skills", "app");
            });

            modelBuilder.Entity<HitPointModel>(action =>
            {
                action.ToTable("HitPoints", "app");
            });

            modelBuilder.Entity<HitDiceModel>(action =>
            {
                action.ToTable("HitDices", "app");
            });

            modelBuilder.Entity<DeathSaveThrowModel>(action =>
            {
                action.ToTable("DeathSaveThrows", "app");
            });

            modelBuilder.Entity<AbilitySaveThrowModel>(action =>
            {
                action.ToTable("AbilitySaveThrows", "app");
            });

            modelBuilder.Entity<ToolModel>(action =>
            {
                action.ToTable("Tools", "app");
            });

            modelBuilder.Entity<OtherToolModel>(action =>
            {
                action.ToTable("OtherTools", "app");
            });

            modelBuilder.Entity<AttackModel>(action =>
            {
                action.ToTable("Attacks", "app");
            });

            modelBuilder.Entity<GlobalDamageModifierModel>(action =>
            {
                action.ToTable("GlobalDamageModifiers", "app");
            });

            modelBuilder.Entity<InventoryGoldModel>(action =>
            {
                action.ToTable("InventoryGold", "app");
            });

            modelBuilder.Entity<InventoryItemModel>(action =>
            {
                action.ToTable("InventoryItems", "app");
            });

            modelBuilder.Entity<ClassResourceModel>(action =>
            {
                action.ToTable("ClassResources", "app");
            });

            modelBuilder.Entity<OtherResourceModel>(action =>
            {
                action.ToTable("OtherResources", "app");
            });

            modelBuilder.Entity<Sheet2Model>(action =>
            {
                action.ToTable("Sheet2", "app");
            });

            modelBuilder.Entity<Sheet3Model>(action =>
            {
                action.ToTable("Sheet3", "app");
            });

            // Configure public schema models here.
        }
    }
}
