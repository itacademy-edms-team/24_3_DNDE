using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTrack.Finance.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_TelegramBotLinkingSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTelegramNotificationsEnabled",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "TelegramChatId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TelegramBotLinkingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PrimaryCode = table.Column<Guid>(type: "uuid", nullable: false),
                    ConfirmationCode = table.Column<int>(type: "integer", nullable: true),
                    TelegramChatId = table.Column<long>(type: "bigint", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramBotLinkingSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TelegramBotLinkingSessions_PrimaryCode",
                table: "TelegramBotLinkingSessions",
                column: "PrimaryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelegramBotLinkingSessions_UserId",
                table: "TelegramBotLinkingSessions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TelegramBotLinkingSessions");

            migrationBuilder.DropColumn(
                name: "IsTelegramNotificationsEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TelegramChatId",
                table: "Users");
        }
    }
}
