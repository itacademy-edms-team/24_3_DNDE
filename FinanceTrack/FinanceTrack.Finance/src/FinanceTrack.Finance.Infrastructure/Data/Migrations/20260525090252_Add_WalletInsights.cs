using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTrack.Finance.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_WalletInsights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WalletInsights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    InsightMonth = table.Column<DateOnly>(type: "date", nullable: false),
                    AnomaliesText = table.Column<string>(type: "text", nullable: true),
                    TrendsText = table.Column<string>(type: "text", nullable: true),
                    ExpenseStructureText = table.Column<string>(type: "text", nullable: true),
                    RecommendationsText = table.Column<string>(type: "text", nullable: true),
                    GeneratedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletInsights", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WalletInsights_WalletId_UserId",
                table: "WalletInsights",
                columns: new[] { "WalletId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_WalletInsights_WalletId_UserId_InsightMonth",
                table: "WalletInsights",
                columns: new[] { "WalletId", "UserId", "InsightMonth" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WalletInsights");
        }
    }
}
