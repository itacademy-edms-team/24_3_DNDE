using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sets.Infrastructure.Data.Outbox;

/// <summary>
/// Схема outbox-сообщений: порядок задаёт identity-идентификатор, незавершённые ищутся
/// по частичному индексу (ADR 0040).
/// </summary>
internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(message => message.Id);
        builder.Property(message => message.Id)
            .HasColumnName("id")
            .UseIdentityAlwaysColumn();

        builder.Property(message => message.Type)
            .HasColumnName("type")
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(message => message.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();
        builder.Property(message => message.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        builder.Property(message => message.NextAttemptAt)
            .HasColumnName("next_attempt_at")
            .IsRequired();
        builder.Property(message => message.Attempts)
            .HasColumnName("attempts")
            .IsRequired();
        builder.Property(message => message.ProcessedAt)
            .HasColumnName("processed_at");
        builder.Property(message => message.AbandonedAt)
            .HasColumnName("abandoned_at");
        builder.Property(message => message.LastError)
            .HasColumnName("last_error").HasMaxLength(1024);

        builder.HasIndex(message => new { message.NextAttemptAt, message.Id })
            .HasDatabaseName("ix_outbox_messages_pending")
            .HasFilter("processed_at IS NULL AND abandoned_at IS NULL");
    }
}
