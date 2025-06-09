using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DND5E_CE.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddSheet3Spell : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Spells",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sheet3Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    School = table.Column<string>(type: "text", nullable: false),
                    IsRitual = table.Column<bool>(type: "boolean", nullable: false),
                    CastingTime = table.Column<string>(type: "text", nullable: false),
                    Range = table.Column<string>(type: "text", nullable: false),
                    Target = table.Column<string>(type: "text", nullable: false),
                    ComponentsV = table.Column<bool>(type: "boolean", nullable: false),
                    ComponentsS = table.Column<bool>(type: "boolean", nullable: false),
                    ComponentsM = table.Column<bool>(type: "boolean", nullable: false),
                    ComponentsDescription = table.Column<string>(type: "text", nullable: false),
                    IsConcentration = table.Column<bool>(type: "boolean", nullable: false),
                    Duration = table.Column<string>(type: "text", nullable: false),
                    SpellCastingAbility = table.Column<string>(type: "text", nullable: false),
                    Innate = table.Column<string>(type: "text", nullable: false),
                    Output = table.Column<string>(type: "text", nullable: false),
                    AttackType = table.Column<string>(type: "text", nullable: false),
                    AttackDamage1Dice = table.Column<string>(type: "text", nullable: false),
                    AttackDamage1Type = table.Column<string>(type: "text", nullable: false),
                    AttackDamage2Dice = table.Column<string>(type: "text", nullable: false),
                    AttackDamage2Type = table.Column<string>(type: "text", nullable: false),
                    HealingDice = table.Column<string>(type: "text", nullable: false),
                    IsAbilityModIncluded = table.Column<bool>(type: "boolean", nullable: false),
                    SavingThrowAbility = table.Column<string>(type: "text", nullable: false),
                    SavingThrowEffect = table.Column<string>(type: "text", nullable: false),
                    HigherLevelCastDiceAmount = table.Column<int>(type: "integer", nullable: false),
                    HigherLevelCastDiceType = table.Column<string>(type: "text", nullable: false),
                    HigherLevelCastBonus = table.Column<int>(type: "integer", nullable: false),
                    IncludeSpellDescriptionInAttack = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    AtHigherLevels = table.Column<string>(type: "text", nullable: false),
                    Class = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    CantripProgression = table.Column<string>(type: "text", nullable: false),
                    IsPrepared = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spells_Sheet3_Sheet3Id",
                        column: x => x.Sheet3Id,
                        principalSchema: "app",
                        principalTable: "Sheet3",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Spells_Sheet3Id",
                schema: "app",
                table: "Spells",
                column: "Sheet3Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Spells",
                schema: "app");
        }
    }
}
