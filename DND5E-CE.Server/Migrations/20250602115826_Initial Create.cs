using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DND5E_CE.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "app");

            migrationBuilder.EnsureSchema(
                name: "tokens");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateTable(
                name: "Abilities",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sheet1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StrengthBase = table.Column<int>(type: "integer", nullable: false),
                    DexterityBase = table.Column<int>(type: "integer", nullable: false),
                    ConstitutionBase = table.Column<int>(type: "integer", nullable: false),
                    IntelligenceBase = table.Column<int>(type: "integer", nullable: false),
                    WisdomBase = table.Column<int>(type: "integer", nullable: false),
                    CharismaBase = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Abilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbilitySaveThrows",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sheet1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsSaveThrowStrengthProficient = table.Column<bool>(type: "boolean", nullable: false),
                    IsSaveThrowDexterityProficient = table.Column<bool>(type: "boolean", nullable: false),
                    IsSaveThrowConstitutionProficient = table.Column<bool>(type: "boolean", nullable: false),
                    IsSaveThrowIntelligenceProficient = table.Column<bool>(type: "boolean", nullable: false),
                    IsSaveThrowWisdomProficient = table.Column<bool>(type: "boolean", nullable: false),
                    IsSaveThrowCharismaProficient = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbilitySaveThrows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClassResources",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sheet1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Total = table.Column<int>(type: "integer", nullable: false),
                    Current = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsUsePb = table.Column<bool>(type: "boolean", nullable: false),
                    ResetOn = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassResources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeathSaveThrows",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sheet1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SuccessTotal = table.Column<int>(type: "integer", nullable: false),
                    FailuresTotal = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeathSaveThrows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HitDices",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sheet1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Total = table.Column<int>(type: "integer", nullable: false),
                    Current = table.Column<int>(type: "integer", nullable: false),
                    DiceType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HitDices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HitPoints",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sheet1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Max = table.Column<int>(type: "integer", nullable: false),
                    Current = table.Column<int>(type: "integer", nullable: false),
                    Temp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HitPoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryGold",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sheet1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Cp = table.Column<int>(type: "integer", nullable: false),
                    Sp = table.Column<int>(type: "integer", nullable: false),
                    Ep = table.Column<int>(type: "integer", nullable: false),
                    Gp = table.Column<int>(type: "integer", nullable: false),
                    Pp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryGold", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sheet2",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Age = table.Column<string>(type: "text", nullable: false),
                    Height = table.Column<string>(type: "text", nullable: false),
                    Weight = table.Column<string>(type: "text", nullable: false),
                    Eyes = table.Column<string>(type: "text", nullable: false),
                    Skin = table.Column<string>(type: "text", nullable: false),
                    Hair = table.Column<string>(type: "text", nullable: false),
                    Appearance = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Backstory = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AlliesAndOrganizations = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AdditionalFeaturesAndTraits = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Treasures = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sheet2", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sheet3",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpellBondAbility = table.Column<string>(type: "text", nullable: false),
                    RemainingSpellSlotsLevel1 = table.Column<int>(type: "integer", nullable: false),
                    RemainingSpellSlotsLevel2 = table.Column<int>(type: "integer", nullable: false),
                    RemainingSpellSlotsLevel3 = table.Column<int>(type: "integer", nullable: false),
                    RemainingSpellSlotsLevel4 = table.Column<int>(type: "integer", nullable: false),
                    RemainingSpellSlotsLevel5 = table.Column<int>(type: "integer", nullable: false),
                    RemainingSpellSlotsLevel6 = table.Column<int>(type: "integer", nullable: false),
                    RemainingSpellSlotsLevel7 = table.Column<int>(type: "integer", nullable: false),
                    RemainingSpellSlotsLevel8 = table.Column<int>(type: "integer", nullable: false),
                    RemainingSpellSlotsLevel9 = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sheet3", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sheet1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAcrobaticsProficient = table.Column<bool>(type: "boolean", nullable: false),
                    AcrobaticsBondAbility = table.Column<string>(type: "text", nullable: false),
                    IsAnimalHandlingProficient = table.Column<bool>(type: "boolean", nullable: false),
                    AnimalHandlingBondAbility = table.Column<string>(type: "text", nullable: false),
                    IsArcanaProficient = table.Column<bool>(type: "boolean", nullable: false),
                    ArcanaBondAbility = table.Column<string>(type: "text", nullable: false),
                    IsAthleticsProficient = table.Column<bool>(type: "boolean", nullable: false),
                    AthleticsBondAbility = table.Column<string>(type: "text", nullable: false),
                    IsDeceptionProficient = table.Column<bool>(type: "boolean", nullable: false),
                    DeceptionBondAbility = table.Column<string>(type: "text", nullable: false),
                    IsHistoryProficient = table.Column<bool>(type: "boolean", nullable: false),
                    HistoryBondAbility = table.Column<string>(type: "text", nullable: false),
                    IsInsightProficient = table.Column<bool>(type: "boolean", nullable: false),
                    InsightBondAbility = table.Column<string>(type: "text", nullable: false),
                    IsIntimidationProficient = table.Column<bool>(type: "boolean", nullable: false),
                    IntimidationBondAbility = table.Column<string>(type: "text", nullable: false),
                    IsInvestigationProficient = table.Column<bool>(type: "boolean", nullable: false),
                    InvestigationBondAbility = table.Column<string>(type: "text", nullable: false),
                    IsMedicineProficient = table.Column<bool>(type: "boolean", nullable: false),
                    MedicineBondAbility = table.Column<string>(type: "text", nullable: false),
                    IsNatureProficient = table.Column<bool>(type: "boolean", nullable: false),
                    NatureBondAbility = table.Column<string>(type: "text", nullable: false),
                    IsPerceptionProficient = table.Column<bool>(type: "boolean", nullable: false),
                    PerceptionBondAbility = table.Column<string>(type: "text", nullable: false),
                    IsPerformanceProficient = table.Column<bool>(type: "boolean", nullable: false),
                    PerformanceBondAbility = table.Column<string>(type: "text", nullable: false),
                    IsPersuasionProficient = table.Column<bool>(type: "boolean", nullable: false),
                    PersuasionBondAbility = table.Column<string>(type: "text", nullable: false),
                    IsReligionProficient = table.Column<bool>(type: "boolean", nullable: false),
                    ReligionBondAbility = table.Column<string>(type: "text", nullable: false),
                    IsSleightOfHandProficient = table.Column<bool>(type: "boolean", nullable: false),
                    SleightOfHandBondAbility = table.Column<string>(type: "text", nullable: false),
                    IsStealthProficient = table.Column<bool>(type: "boolean", nullable: false),
                    StealthBondAbility = table.Column<string>(type: "text", nullable: false),
                    IsSurvivalProficient = table.Column<bool>(type: "boolean", nullable: false),
                    SurvivalBondAbility = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "identity",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sheet1",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Class = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Race = table.Column<string>(type: "text", nullable: false),
                    Backstory = table.Column<string>(type: "text", nullable: false),
                    Worldview = table.Column<string>(type: "text", nullable: false),
                    PlayerName = table.Column<string>(type: "text", nullable: false),
                    Experience = table.Column<int>(type: "integer", nullable: false),
                    AbilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    AbilitySaveThrowId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArmorClass = table.Column<int>(type: "integer", nullable: false),
                    Initiative = table.Column<int>(type: "integer", nullable: false),
                    Speed = table.Column<int>(type: "integer", nullable: false),
                    HitPointId = table.Column<Guid>(type: "uuid", nullable: false),
                    HitDiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeathSaveThrowId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryGoldId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonalityTraits = table.Column<string>(type: "text", nullable: false),
                    Ideals = table.Column<string>(type: "text", nullable: false),
                    Bonds = table.Column<string>(type: "text", nullable: false),
                    Flaws = table.Column<string>(type: "text", nullable: false),
                    ClassResourceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sheet1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sheet1_Abilities_AbilityId",
                        column: x => x.AbilityId,
                        principalSchema: "app",
                        principalTable: "Abilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sheet1_AbilitySaveThrows_AbilitySaveThrowId",
                        column: x => x.AbilitySaveThrowId,
                        principalSchema: "app",
                        principalTable: "AbilitySaveThrows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sheet1_ClassResources_ClassResourceId",
                        column: x => x.ClassResourceId,
                        principalSchema: "app",
                        principalTable: "ClassResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sheet1_DeathSaveThrows_DeathSaveThrowId",
                        column: x => x.DeathSaveThrowId,
                        principalSchema: "app",
                        principalTable: "DeathSaveThrows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sheet1_HitDices_HitDiceId",
                        column: x => x.HitDiceId,
                        principalSchema: "app",
                        principalTable: "HitDices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sheet1_HitPoints_HitPointId",
                        column: x => x.HitPointId,
                        principalSchema: "app",
                        principalTable: "HitPoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sheet1_InventoryGold_InventoryGoldId",
                        column: x => x.InventoryGoldId,
                        principalSchema: "app",
                        principalTable: "InventoryGold",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sheet1_Skills_SkillId",
                        column: x => x.SkillId,
                        principalSchema: "app",
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CsrfTokens",
                schema: "tokens",
                columns: table => new
                {
                    Token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CsrfTokens", x => x.Token);
                    table.ForeignKey(
                        name: "FK_CsrfTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                schema: "tokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "text", nullable: false),
                    JwtId = table.Column<string>(type: "text", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                schema: "identity",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "identity",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "identity",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                schema: "identity",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attacks",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sheet1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    AtkIsIncluded = table.Column<bool>(type: "boolean", nullable: false),
                    AtkBondAbility = table.Column<string>(type: "text", nullable: false),
                    AtkBonus = table.Column<int>(type: "integer", nullable: false),
                    AtkIsProficient = table.Column<bool>(type: "boolean", nullable: false),
                    AtkRange = table.Column<string>(type: "text", nullable: false),
                    AtkMagicBonus = table.Column<int>(type: "integer", nullable: false),
                    AtkCritRange = table.Column<string>(type: "text", nullable: false),
                    Damage1IsIncluded = table.Column<bool>(type: "boolean", nullable: false),
                    Damage1Dice = table.Column<string>(type: "text", nullable: false),
                    Damage1BondAbility = table.Column<string>(type: "text", nullable: false),
                    Damage1Bonus = table.Column<int>(type: "integer", nullable: false),
                    Damage1Type = table.Column<string>(type: "text", nullable: false),
                    Damage1CritDice = table.Column<string>(type: "text", nullable: false),
                    Damage2IsIncluded = table.Column<bool>(type: "boolean", nullable: false),
                    Damage2Dice = table.Column<string>(type: "text", nullable: false),
                    Damage2BondAbility = table.Column<string>(type: "text", nullable: false),
                    Damage2Bonus = table.Column<int>(type: "integer", nullable: false),
                    Damage2Type = table.Column<string>(type: "text", nullable: false),
                    Damage2CritDice = table.Column<string>(type: "text", nullable: false),
                    SavingThrowIsIncluded = table.Column<bool>(type: "boolean", nullable: false),
                    SavingThrowBondAbility = table.Column<string>(type: "text", nullable: false),
                    SavingThrowDifficultyClass = table.Column<string>(type: "text", nullable: false),
                    SaveEffect = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attacks_Sheet1_Sheet1Id",
                        column: x => x.Sheet1Id,
                        principalSchema: "app",
                        principalTable: "Sheet1",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Sheet1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sheet2Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sheet3Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Characters_Sheet1_Sheet1Id",
                        column: x => x.Sheet1Id,
                        principalSchema: "app",
                        principalTable: "Sheet1",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Characters_Sheet2_Sheet2Id",
                        column: x => x.Sheet2Id,
                        principalSchema: "app",
                        principalTable: "Sheet2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Characters_Sheet3_Sheet3Id",
                        column: x => x.Sheet3Id,
                        principalSchema: "app",
                        principalTable: "Sheet3",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Characters_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GlobalDamageModifiers",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sheet1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DamageDice = table.Column<string>(type: "text", nullable: false),
                    CriticalDamageDice = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    IsIncluded = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalDamageModifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlobalDamageModifiers_Sheet1_Sheet1Id",
                        column: x => x.Sheet1Id,
                        principalSchema: "app",
                        principalTable: "Sheet1",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sheet1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false),
                    IsEquipped = table.Column<bool>(type: "boolean", nullable: false),
                    IsUsedAsResource = table.Column<bool>(type: "boolean", nullable: false),
                    IsHasAnAttack = table.Column<bool>(type: "boolean", nullable: false),
                    Prop = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryItems_Sheet1_Sheet1Id",
                        column: x => x.Sheet1Id,
                        principalSchema: "app",
                        principalTable: "Sheet1",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtherResources",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sheet1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Total = table.Column<int>(type: "integer", nullable: false),
                    Current = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsUsePb = table.Column<bool>(type: "boolean", nullable: false),
                    ResetOn = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtherResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtherResources_Sheet1_Sheet1Id",
                        column: x => x.Sheet1Id,
                        principalSchema: "app",
                        principalTable: "Sheet1",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OtherTools",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sheet1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtherTools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtherTools_Sheet1_Sheet1Id",
                        column: x => x.Sheet1Id,
                        principalSchema: "app",
                        principalTable: "Sheet1",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tools",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sheet1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ProficiencyType = table.Column<string>(type: "text", nullable: false),
                    BondAbility = table.Column<string>(type: "text", nullable: false),
                    Mods = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tools_Sheet1_Sheet1Id",
                        column: x => x.Sheet1Id,
                        principalSchema: "app",
                        principalTable: "Sheet1",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attacks_Sheet1Id",
                schema: "app",
                table: "Attacks",
                column: "Sheet1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_Sheet1Id",
                schema: "app",
                table: "Characters",
                column: "Sheet1Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Characters_Sheet2Id",
                schema: "app",
                table: "Characters",
                column: "Sheet2Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Characters_Sheet3Id",
                schema: "app",
                table: "Characters",
                column: "Sheet3Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Characters_UserId",
                schema: "app",
                table: "Characters",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CsrfTokens_Token_UserId",
                schema: "tokens",
                table: "CsrfTokens",
                columns: new[] { "Token", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CsrfTokens_UserId",
                schema: "tokens",
                table: "CsrfTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalDamageModifiers_Sheet1Id",
                schema: "app",
                table: "GlobalDamageModifiers",
                column: "Sheet1Id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_Sheet1Id",
                schema: "app",
                table: "InventoryItems",
                column: "Sheet1Id");

            migrationBuilder.CreateIndex(
                name: "IX_OtherResources_Sheet1Id",
                schema: "app",
                table: "OtherResources",
                column: "Sheet1Id");

            migrationBuilder.CreateIndex(
                name: "IX_OtherTools_Sheet1Id",
                schema: "app",
                table: "OtherTools",
                column: "Sheet1Id");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                schema: "tokens",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                schema: "tokens",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                schema: "identity",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "identity",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sheet1_AbilityId",
                schema: "app",
                table: "Sheet1",
                column: "AbilityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sheet1_AbilitySaveThrowId",
                schema: "app",
                table: "Sheet1",
                column: "AbilitySaveThrowId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sheet1_ClassResourceId",
                schema: "app",
                table: "Sheet1",
                column: "ClassResourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sheet1_DeathSaveThrowId",
                schema: "app",
                table: "Sheet1",
                column: "DeathSaveThrowId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sheet1_HitDiceId",
                schema: "app",
                table: "Sheet1",
                column: "HitDiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sheet1_HitPointId",
                schema: "app",
                table: "Sheet1",
                column: "HitPointId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sheet1_InventoryGoldId",
                schema: "app",
                table: "Sheet1",
                column: "InventoryGoldId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sheet1_SkillId",
                schema: "app",
                table: "Sheet1",
                column: "SkillId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tools_Sheet1Id",
                schema: "app",
                table: "Tools",
                column: "Sheet1Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                schema: "identity",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                schema: "identity",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                schema: "identity",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "identity",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "identity",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attacks",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Characters",
                schema: "app");

            migrationBuilder.DropTable(
                name: "CsrfTokens",
                schema: "tokens");

            migrationBuilder.DropTable(
                name: "GlobalDamageModifiers",
                schema: "app");

            migrationBuilder.DropTable(
                name: "InventoryItems",
                schema: "app");

            migrationBuilder.DropTable(
                name: "OtherResources",
                schema: "app");

            migrationBuilder.DropTable(
                name: "OtherTools",
                schema: "app");

            migrationBuilder.DropTable(
                name: "RefreshTokens",
                schema: "tokens");

            migrationBuilder.DropTable(
                name: "RoleClaims",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "Tools",
                schema: "app");

            migrationBuilder.DropTable(
                name: "UserClaims",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserLogins",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserTokens",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "Sheet2",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Sheet3",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Sheet1",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "Abilities",
                schema: "app");

            migrationBuilder.DropTable(
                name: "AbilitySaveThrows",
                schema: "app");

            migrationBuilder.DropTable(
                name: "ClassResources",
                schema: "app");

            migrationBuilder.DropTable(
                name: "DeathSaveThrows",
                schema: "app");

            migrationBuilder.DropTable(
                name: "HitDices",
                schema: "app");

            migrationBuilder.DropTable(
                name: "HitPoints",
                schema: "app");

            migrationBuilder.DropTable(
                name: "InventoryGold",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Skills",
                schema: "app");
        }
    }
}
