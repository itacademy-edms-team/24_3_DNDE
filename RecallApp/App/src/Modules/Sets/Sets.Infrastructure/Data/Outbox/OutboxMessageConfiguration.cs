using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sets.Infrastructure.Data.Outbox;

/// <summary>
/// Схема outbox-сообщений: identity-идентификатор задаёт порядок, незавершённые ищутся
/// по частичному индексу (полоса, время следующей попытки).
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
        builder.Property(message => message.Version)
            .HasColumnName("version")
            .IsRequired();
        builder.Property(message => message.Lane)
            .HasColumnName("lane")
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(message => message.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();
        builder.Property(message => message.Status)
            .HasColumnName("status")
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(message => message.Checkpoint)
            .HasColumnName("checkpoint")
            .HasColumnType("jsonb");
        builder.Property(message => message.Attempts)
            .HasColumnName("attempts")
            .IsRequired();
        builder.Property(message => message.NextAttemptAt)
            .HasColumnName("next_attempt_at")
            .IsRequired();
        builder.Property(message => message.LeaseOwner)
            .HasColumnName("lease_owner");
        builder.Property(message => message.LeaseUntil)
            .HasColumnName("lease_until");
        builder.Property(message => message.LastError)
            .HasColumnName("last_error")
            .HasMaxLength(1024);
        builder.Property(message => message.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        builder.Property(message => message.LastProcessedAt)
            .HasColumnName("last_processed_at");
        builder.Property(message => message.FinishedAt)
            .HasColumnName("finished_at");

        builder.HasIndex(message => new { message.Lane, message.NextAttemptAt, message.Id })
            .HasDatabaseName("ix_outbox_messages_pending")
            .HasFilter("status IN ('pending', 'retrying')");
    }
}
