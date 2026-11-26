using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Common.Infrastructure.Migrations.Logging
{
    /// <inheritdoc />
    public partial class InitialLogging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "logging");

            migrationBuilder.CreateTable(
                name: "http_logs",
                schema: "logging",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Duration = table.Column<double>(type: "double precision", nullable: true),
                    Level = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Logger = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    MessageProperties = table.Column<string>(type: "text", nullable: true),
                    ExceptionType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ExceptionMessage = table.Column<string>(type: "text", nullable: true),
                    RootExceptionType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RootExceptionMessage = table.Column<string>(type: "text", nullable: true),
                    StackTraceShort = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    StackTraceCompressed = table.Column<byte[]>(type: "bytea", nullable: true),
                    ClientIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    HttpMethod = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    RequestPath = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    RequestQuery = table.Column<string>(type: "text", nullable: true),
                    ResponseStatusCode = table.Column<int>(type: "integer", nullable: true),
                    RequestHeadersShort = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RequestHeadersCompressed = table.Column<byte[]>(type: "bytea", nullable: true),
                    RequestBodyShort = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RequestBodyCompressed = table.Column<byte[]>(type: "bytea", nullable: true),
                    ResponseHeadersShort = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ResponseHeadersCompressed = table.Column<byte[]>(type: "bytea", nullable: true),
                    ResponseBodyShort = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ResponseBodyCompressed = table.Column<byte[]>(type: "bytea", nullable: true),
                    TraceId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ParentSpanId = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    SpanId = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    SpanKind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    ServiceName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_http_logs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_http_logs_StartTime",
                schema: "logging",
                table: "http_logs",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_http_logs_TraceId",
                schema: "logging",
                table: "http_logs",
                column: "TraceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "http_logs",
                schema: "logging");
        }
    }
}
