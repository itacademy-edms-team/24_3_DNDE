using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTrack.Finance.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_User_LastReminderDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "LastEmailReminderDate",
                table: "RecurringTransactions",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsEmailNotificationsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropColumn(
                name: "LastEmailReminderDate",
                table: "RecurringTransactions");
        }
    }
}
