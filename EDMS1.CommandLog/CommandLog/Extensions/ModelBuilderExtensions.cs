using EDMS1.CommandLog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EDMS1.CommandLog.Extensions;

/// <summary>
/// Набор расширений для конфигурации таблиц CommandLog в хосте.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Добавляет таблицу журнала команд (<see cref="CommandLogEntry"/>) в модель контекста хоста.
    /// Вызывать в <see cref="DbContext.OnModelCreating"/> того контекста,
    /// через который будет работать <c>CommandLogService</c>.
    /// </summary>
    /// <param name="builder">Строитель модели контекста хоста.</param>
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