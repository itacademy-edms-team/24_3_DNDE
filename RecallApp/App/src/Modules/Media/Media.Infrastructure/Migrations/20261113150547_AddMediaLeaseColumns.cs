using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Media.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaLeaseColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "lease_owner",
                schema: "media",
                table: "idempotency_keys",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "lease_until",
                schema: "media",
                table: "idempotency_keys",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                schema: "media",
                table: "idempotency_keys",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lease_owner",
                schema: "media",
                table: "idempotency_keys");

            migrationBuilder.DropColumn(
                name: "lease_until",
                schema: "media",
                table: "idempotency_keys");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "media",
                table: "idempotency_keys");
        }
    }
}
