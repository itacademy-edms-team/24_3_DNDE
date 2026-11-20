using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sets.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sets");

            migrationBuilder.CreateTable(
                name: "sets",
                schema: "sets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sets", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_sets_owner_id_updated_at",
                schema: "sets",
                table: "sets",
                columns: new[] { "owner_id", "updated_at" },
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sets",
                schema: "sets");
        }
    }
}
