using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DND5E_CE.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddSheetModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "app");

            migrationBuilder.CreateTable(
                name: "Sheet1Models",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Class = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Race = table.Column<string>(type: "text", nullable: false),
                    Backstory = table.Column<string>(type: "text", nullable: false),
                    Worldview = table.Column<string>(type: "text", nullable: false),
                    PlayerName = table.Column<string>(type: "text", nullable: false),
                    Experience = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sheet1Models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sheet1Models_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sheet2Models",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_Sheet2Models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sheet2Models_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sheet3Models",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_Sheet3Models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sheet3Models_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterModels",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Sheet1Id = table.Column<int>(type: "integer", nullable: false),
                    Sheet2Id = table.Column<int>(type: "integer", nullable: false),
                    Sheet3Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterModels_Sheet1Models_Sheet1Id",
                        column: x => x.Sheet1Id,
                        principalSchema: "app",
                        principalTable: "Sheet1Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterModels_Sheet2Models_Sheet2Id",
                        column: x => x.Sheet2Id,
                        principalSchema: "app",
                        principalTable: "Sheet2Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterModels_Sheet3Models_Sheet3Id",
                        column: x => x.Sheet3Id,
                        principalSchema: "app",
                        principalTable: "Sheet3Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterModels_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterModels_Sheet1Id",
                schema: "app",
                table: "CharacterModels",
                column: "Sheet1Id");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterModels_Sheet2Id",
                schema: "app",
                table: "CharacterModels",
                column: "Sheet2Id");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterModels_Sheet3Id",
                schema: "app",
                table: "CharacterModels",
                column: "Sheet3Id");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterModels_UserId",
                schema: "app",
                table: "CharacterModels",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sheet1Models_UserId",
                schema: "app",
                table: "Sheet1Models",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sheet2Models_UserId",
                schema: "app",
                table: "Sheet2Models",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sheet3Models_UserId",
                schema: "app",
                table: "Sheet3Models",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterModels",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Sheet1Models",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Sheet2Models",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Sheet3Models",
                schema: "app");
        }
    }
}
