using EDMS1.CommandLog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EDMS1.CommandLog.Extensions;

public static class ModelBuilderExtensions
{
    public static void UseCommandLog(this ModelBuilder builder)
    {
        builder.Entity<CommandLogEntry>(ConfigureLogEntry);

        static void ConfigureLogEntry(EntityTypeBuilder<CommandLogEntry> builder)
        {
            builder.ToTable("CommandLog");

            builder.HasKey(e => e.CommandLogId);

            builder.Property(e => e.CommandLogId)
                .IsRequired();

            builder.Property(e => e.CreationTime)
                .IsRequired();

            builder.Property(e => e.CorrelationToken)
                .IsRequired();

            builder.Property(e => e.TransactionId)
                .IsRequired();

            builder.Property(e => e.CommandName)
                .IsRequired();

            builder.Property(e => e.Command)
                .IsRequired();

            builder.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(e => e.FailureMessage);

            builder.HasIndex(e => e.CreationTime);

            builder.HasIndex(e => e.Status);
        }
    }
}