using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Media.Infrastructure.Data.Idempotency;

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
        builder.Property(record => record.Status).HasColumnName("status").HasMaxLength(16);
        builder.Property(record => record.LeaseOwner).HasColumnName("lease_owner");
        builder.Property(record => record.LeaseUntil).HasColumnName("lease_until");
        builder.Property(record => record.ExpiresAt).HasColumnName("expires_at");
        builder.Property(record => record.CreatedAt).HasColumnName("created_at");

        // Фоновая чистка идёт по expires_at.
        builder.HasIndex(record => record.ExpiresAt).HasDatabaseName("ix_idempotency_keys_expires_at");
    }
}
