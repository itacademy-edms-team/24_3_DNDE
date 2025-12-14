using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTrack.Finance.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.CreateTable(
                name: "FinancialTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IncomeTransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OperationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsMonthly = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_FinancialTransactions_IncomeTransacti~",
                        column: x => x.IncomeTransactionId,
                        principalTable: "FinancialTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_IncomeTransactionId",
                table: "FinancialTransactions",
                column: "IncomeTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_OperationDate",
                table: "FinancialTransactions",
                column: "OperationDate");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_TransactionType",
                table: "FinancialTransactions",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_UserId",
                table: "FinancialTransactions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialTransactions");

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IncomeTransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    IsMonthly = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OperationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Transactions_IncomeTransactionId",
                        column: x => x.IncomeTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_IncomeTransactionId",
                table: "Transactions",
                column: "IncomeTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_OperationDate",
                table: "Transactions",
                column: "OperationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionType",
                table: "Transactions",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                column: "UserId");
        }
    }
}
