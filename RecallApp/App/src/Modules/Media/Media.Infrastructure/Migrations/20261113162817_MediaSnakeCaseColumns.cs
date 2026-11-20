using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Media.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MediaSnakeCaseColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_media_references_media_objects_MediaId",
                schema: "media",
                table: "media_references");

            migrationBuilder.DropForeignKey(
                name: "FK_media_variants_media_objects_MediaId",
                schema: "media",
                table: "media_variants");

            migrationBuilder.RenameColumn(
                name: "Width",
                schema: "media",
                table: "media_variants",
                newName: "width");

            migrationBuilder.RenameColumn(
                name: "Height",
                schema: "media",
                table: "media_variants",
                newName: "height");

            migrationBuilder.RenameColumn(
                name: "Variant",
                schema: "media",
                table: "media_variants",
                newName: "variant");

            migrationBuilder.RenameColumn(
                name: "SizeBytes",
                schema: "media",
                table: "media_variants",
                newName: "size_bytes");

            migrationBuilder.RenameColumn(
                name: "ObjectKey",
                schema: "media",
                table: "media_variants",
                newName: "object_key");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "media",
                table: "media_variants",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "MediaId",
                schema: "media",
                table: "media_variants",
                newName: "media_id");

            migrationBuilder.RenameColumn(
                name: "Reference",
                schema: "media",
                table: "media_references",
                newName: "reference");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "media",
                table: "media_references",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "media",
                table: "media_references",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "MediaId",
                schema: "media",
                table: "media_references",
                newName: "media_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "media",
                table: "media_references",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Width",
                schema: "media",
                table: "media_objects",
                newName: "width");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "media",
                table: "media_objects",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Height",
                schema: "media",
                table: "media_objects",
                newName: "height");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "media",
                table: "media_objects",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UploadExpiresAt",
                schema: "media",
                table: "media_objects",
                newName: "upload_expires_at");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "media",
                table: "media_objects",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "SourceSizeBytes",
                schema: "media",
                table: "media_objects",
                newName: "source_size_bytes");

            migrationBuilder.RenameColumn(
                name: "SourceFormat",
                schema: "media",
                table: "media_objects",
                newName: "source_format");

            migrationBuilder.RenameColumn(
                name: "ReadyAt",
                schema: "media",
                table: "media_objects",
                newName: "ready_at");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                schema: "media",
                table: "media_objects",
                newName: "owner_id");

            migrationBuilder.RenameColumn(
                name: "FailureDescription",
                schema: "media",
                table: "media_objects",
                newName: "failure_description");

            migrationBuilder.RenameColumn(
                name: "FailureCode",
                schema: "media",
                table: "media_objects",
                newName: "failure_code");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                schema: "media",
                table: "media_objects",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "DeclaredContentType",
                schema: "media",
                table: "media_objects",
                newName: "declared_content_type");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "media",
                table: "media_objects",
                newName: "created_at");

            migrationBuilder.AddForeignKey(
                name: "FK_media_references_media_objects_media_id",
                schema: "media",
                table: "media_references",
                column: "media_id",
                principalSchema: "media",
                principalTable: "media_objects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_media_variants_media_objects_media_id",
                schema: "media",
                table: "media_variants",
                column: "media_id",
                principalSchema: "media",
                principalTable: "media_objects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_media_references_media_objects_media_id",
                schema: "media",
                table: "media_references");

            migrationBuilder.DropForeignKey(
                name: "FK_media_variants_media_objects_media_id",
                schema: "media",
                table: "media_variants");

            migrationBuilder.RenameColumn(
                name: "width",
                schema: "media",
                table: "media_variants",
                newName: "Width");

            migrationBuilder.RenameColumn(
                name: "height",
                schema: "media",
                table: "media_variants",
                newName: "Height");

            migrationBuilder.RenameColumn(
                name: "variant",
                schema: "media",
                table: "media_variants",
                newName: "Variant");

            migrationBuilder.RenameColumn(
                name: "size_bytes",
                schema: "media",
                table: "media_variants",
                newName: "SizeBytes");

            migrationBuilder.RenameColumn(
                name: "object_key",
                schema: "media",
                table: "media_variants",
                newName: "ObjectKey");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "media",
                table: "media_variants",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "media_id",
                schema: "media",
                table: "media_variants",
                newName: "MediaId");

            migrationBuilder.RenameColumn(
                name: "reference",
                schema: "media",
                table: "media_references",
                newName: "Reference");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "media",
                table: "media_references",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "media",
                table: "media_references",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "media_id",
                schema: "media",
                table: "media_references",
                newName: "MediaId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "media",
                table: "media_references",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "width",
                schema: "media",
                table: "media_objects",
                newName: "Width");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "media",
                table: "media_objects",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "height",
                schema: "media",
                table: "media_objects",
                newName: "Height");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "media",
                table: "media_objects",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "upload_expires_at",
                schema: "media",
                table: "media_objects",
                newName: "UploadExpiresAt");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "media",
                table: "media_objects",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "source_size_bytes",
                schema: "media",
                table: "media_objects",
                newName: "SourceSizeBytes");

            migrationBuilder.RenameColumn(
                name: "source_format",
                schema: "media",
                table: "media_objects",
                newName: "SourceFormat");

            migrationBuilder.RenameColumn(
                name: "ready_at",
                schema: "media",
                table: "media_objects",
                newName: "ReadyAt");

            migrationBuilder.RenameColumn(
                name: "owner_id",
                schema: "media",
                table: "media_objects",
                newName: "OwnerId");

            migrationBuilder.RenameColumn(
                name: "failure_description",
                schema: "media",
                table: "media_objects",
                newName: "FailureDescription");

            migrationBuilder.RenameColumn(
                name: "failure_code",
                schema: "media",
                table: "media_objects",
                newName: "FailureCode");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                schema: "media",
                table: "media_objects",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "declared_content_type",
                schema: "media",
                table: "media_objects",
                newName: "DeclaredContentType");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "media",
                table: "media_objects",
                newName: "CreatedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_media_references_media_objects_MediaId",
                schema: "media",
                table: "media_references",
                column: "MediaId",
                principalSchema: "media",
                principalTable: "media_objects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_media_variants_media_objects_MediaId",
                schema: "media",
                table: "media_variants",
                column: "MediaId",
                principalSchema: "media",
                principalTable: "media_objects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
