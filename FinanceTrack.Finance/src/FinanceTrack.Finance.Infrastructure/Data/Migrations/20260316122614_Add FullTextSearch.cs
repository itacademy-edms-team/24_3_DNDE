using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace FinanceTrack.Finance.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Wallets",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "to_tsvector('russian', coalesce(\"Name\", ''))",
                stored: true);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "RecurringTransactions",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "to_tsvector('russian', coalesce(\"Name\", ''))",
                stored: true);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "FinancialTransactions",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "to_tsvector('russian', coalesce(\"Name\", ''))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_SearchVector",
                table: "Wallets",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_SearchVector",
                table: "RecurringTransactions",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_SearchVector",
                table: "FinancialTransactions",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wallets_SearchVector",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_RecurringTransactions_SearchVector",
                table: "RecurringTransactions");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_SearchVector",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "RecurringTransactions");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "FinancialTransactions");
        }
    }
}
