using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sets.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetDescriptionRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Существующие null становятся пустой строкой. Пустая строка, это строка "", а не null.
            migrationBuilder.Sql("UPDATE sets.sets SET description = '' WHERE description IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                schema: "sets",
                table: "sets",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "description",
                schema: "sets",
                table: "sets",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);
        }
    }
}
