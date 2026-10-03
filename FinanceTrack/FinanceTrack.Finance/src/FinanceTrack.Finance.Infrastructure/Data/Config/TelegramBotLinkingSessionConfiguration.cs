using FinanceTrack.Finance.Core.TelegramBotLinkingSessionAggregate;

namespace FinanceTrack.Finance.Infrastructure.Data.Config;

public class TelegramBotLinkingSessionConfiguration
    : IEntityTypeConfiguration<TelegramBotLinkingSession>
{
    public void Configure(EntityTypeBuilder<TelegramBotLinkingSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId).IsRequired().HasMaxLength(256);
        builder.Property(s => s.PrimaryCode).IsRequired();
        builder.Property(s => s.ConfirmationCode);

        builder
            .Property(s => s.ExpiresAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder
            .Property(s => s.CreatedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(s => s.PrimaryCode).IsUnique();
        builder.HasIndex(s => s.UserId);
    }
}
