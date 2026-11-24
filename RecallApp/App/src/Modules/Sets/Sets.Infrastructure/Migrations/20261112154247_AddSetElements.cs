using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sets.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSetElements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "elements",
                schema: "sets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_elements", x => x.id);
                    table.ForeignKey(
                        name: "fk_elements_sets_set_id",
                        column: x => x.set_id,
                        principalSchema: "sets",
                        principalTable: "sets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_elements_set_id_position_id",
                schema: "sets",
                table: "elements",
                columns: new[] { "set_id", "position", "id" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ux_elements_set_id_position",
                schema: "sets",
                table: "elements",
                columns: new[] { "set_id", "position" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "elements",
                schema: "sets");
        }
    }
}
