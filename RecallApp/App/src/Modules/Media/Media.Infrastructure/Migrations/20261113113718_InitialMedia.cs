using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Media.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "media");

            migrationBuilder.CreateTable(
                name: "media_objects",
                schema: "media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    DeclaredContentType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceFormat = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    SourceSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    FailureCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FailureDescription = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    UploadExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReadyAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_objects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "media_references",
                schema: "media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_references", x => x.Id);
                    table.ForeignKey(
                        name: "FK_media_references_media_objects_MediaId",
                        column: x => x.MediaId,
                        principalSchema: "media",
                        principalTable: "media_objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media_variants",
                schema: "media",
                columns: table => new
                {
                    MediaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Variant = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ObjectKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_variants", x => new { x.MediaId, x.Variant });
                    table.ForeignKey(
                        name: "FK_media_variants_media_objects_MediaId",
                        column: x => x.MediaId,
                        principalSchema: "media",
                        principalTable: "media_objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_media_objects_owner_id_created_at",
                schema: "media",
                table: "media_objects",
                columns: new[] { "OwnerId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_media_objects_status_created_at",
                schema: "media",
                table: "media_objects",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "ix_media_objects_status_ready_at",
                schema: "media",
                table: "media_objects",
                columns: new[] { "Status", "ReadyAt" });

            migrationBuilder.CreateIndex(
                name: "ix_media_references_media_id",
                schema: "media",
                table: "media_references",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "ux_media_references_reference",
                schema: "media",
                table: "media_references",
                column: "Reference",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media_references",
                schema: "media");

            migrationBuilder.DropTable(
                name: "media_variants",
                schema: "media");

            migrationBuilder.DropTable(
                name: "media_objects",
                schema: "media");
        }
    }
}
