using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTrack.Finance.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FT_Increase_TransactionName_MaxLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // RecurringTransactions: drop SearchVector, alter Name, recreate SearchVector
            migrationBuilder.DropColumn(name: "SearchVector", table: "RecurringTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "RecurringTransactions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "SearchVector",
                table: "RecurringTransactions",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('russian', coalesce(\"Name\", ''))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_SearchVector",
                table: "RecurringTransactions",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            // FinancialTransactions: drop SearchVector, alter Name, recreate SearchVector
            migrationBuilder.DropColumn(name: "SearchVector", table: "FinancialTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "FinancialTransactions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "SearchVector",
                table: "FinancialTransactions",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('russian', coalesce(\"Name\", '') || ' ' || coalesce(\"Description\", ''))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_SearchVector",
                table: "FinancialTransactions",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "SearchVector", table: "RecurringTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "RecurringTransactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "SearchVector",
                table: "RecurringTransactions",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('russian', coalesce(\"Name\", ''))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_SearchVector",
                table: "RecurringTransactions",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.DropColumn(name: "SearchVector", table: "FinancialTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "FinancialTransactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "SearchVector",
                table: "FinancialTransactions",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('russian', coalesce(\"Name\", '') || ' ' || coalesce(\"Description\", ''))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_SearchVector",
                table: "FinancialTransactions",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }
    }
}
