using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sets.Infrastructure.Data.Idempotency;

internal sealed class IdempotencyKeyConfiguration : IEntityTypeConfiguration<IdempotencyKeyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyKeyRecord> builder)
    {
        builder.ToTable("idempotency_keys");
        builder.HasKey(record => new { record.OwnerId, record.Key });

        builder.Property(record => record.OwnerId).HasColumnName("owner_id");
        builder.Property(record => record.Key).HasColumnName("key").HasMaxLength(64);
        builder.Property(record => record.Operation).HasColumnName("operation").HasMaxLength(64).IsRequired();
        builder.Property(record => record.RequestHash).HasColumnName("request_hash").HasMaxLength(64).IsRequired();
        builder.Property(record => record.StatusCode).HasColumnName("status_code");
        builder.Property(record => record.Response).HasColumnName("response").HasColumnType("jsonb");
        builder.Property(record => record.ExpiresAt).HasColumnName("expires_at");
        builder.Property(record => record.CreatedAt).HasColumnName("created_at");

        // Фоновая чистка идёт по expires_at (ADR 0036).
        builder.HasIndex(record => record.ExpiresAt).HasDatabaseName("ix_idempotency_keys_expires_at");
    }
}
