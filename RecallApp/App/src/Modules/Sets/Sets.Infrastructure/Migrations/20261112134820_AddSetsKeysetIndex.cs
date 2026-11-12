using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sets.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSetsKeysetIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_sets_owner_id_updated_at",
                schema: "sets",
                table: "sets");

            migrationBuilder.CreateIndex(
                name: "ix_sets_owner_id_created_at_id",
                schema: "sets",
                table: "sets",
                columns: new[] { "owner_id", "created_at", "id" },
                descending: new[] { false, true, true },
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_sets_owner_id_created_at_id",
                schema: "sets",
                table: "sets");

            migrationBuilder.CreateIndex(
                name: "ix_sets_owner_id_updated_at",
                schema: "sets",
                table: "sets",
                columns: new[] { "owner_id", "updated_at" },
                filter: "deleted_at IS NULL");
        }
    }
}
