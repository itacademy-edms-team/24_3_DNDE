using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DND5E_CE.Server.Migrations
{
    /// <inheritdoc />
    public partial class FixRefreshTokenModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsed",
                schema: "tokens",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "JwtId",
                schema: "tokens",
                table: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "Token",
                schema: "tokens",
                table: "RefreshTokens",
                newName: "TokenHash");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_Token",
                schema: "tokens",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_TokenHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TokenHash",
                schema: "tokens",
                table: "RefreshTokens",
                newName: "Token");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_TokenHash",
                schema: "tokens",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_Token");

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                schema: "tokens",
                table: "RefreshTokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JwtId",
                schema: "tokens",
                table: "RefreshTokens",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
