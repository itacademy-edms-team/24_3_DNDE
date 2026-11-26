using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HttpLogging;

internal sealed class LogEntryConfiguration(string? schema = null) : IEntityTypeConfiguration<LogEntry>
{
    public void Configure(EntityTypeBuilder<LogEntry> builder)
    {
        builder.ToTable("http_logs", schema);
        builder.HasKey(e => e.Id);

        // Голова
        builder.Property(e => e.StartTime).IsRequired();
        builder.Property(e => e.Duration);
        builder.Property(e => e.Level).IsRequired().HasMaxLength(16);
        builder.Property(e => e.Logger).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Message).IsRequired();
        builder.Property(e => e.MessageProperties);
        builder.Property(e => e.ExceptionType).HasMaxLength(256);
        builder.Property(e => e.ExceptionMessage);
        builder.Property(e => e.RootExceptionType).HasMaxLength(256);
        builder.Property(e => e.RootExceptionMessage);
        builder.Property(e => e.StackTraceShort).HasMaxLength(4000);
        builder.Property(e => e.StackTraceCompressed);

        // HTTP, общее
        builder.Property(e => e.ClientIp).HasMaxLength(45);
        builder.Property(e => e.HttpMethod).HasMaxLength(16);
        builder.Property(e => e.RequestPath).HasMaxLength(2048);
        builder.Property(e => e.RequestQuery);
        builder.Property(e => e.ResponseStatusCode);

        // HTTP, содержимое
        builder.Property(e => e.RequestHeadersShort).HasMaxLength(4000);
        builder.Property(e => e.RequestHeadersCompressed);
        builder.Property(e => e.RequestBodyShort).HasMaxLength(4000);
        builder.Property(e => e.RequestBodyCompressed);
        builder.Property(e => e.ResponseHeadersShort).HasMaxLength(4000);
        builder.Property(e => e.ResponseHeadersCompressed);
        builder.Property(e => e.ResponseBodyShort).HasMaxLength(4000);
        builder.Property(e => e.ResponseBodyCompressed);

        // Отслеживаемость
        builder.Property(e => e.TraceId).HasMaxLength(32);
        builder.Property(e => e.ParentSpanId).HasMaxLength(16);
        builder.Property(e => e.SpanId).HasMaxLength(16);
        builder.Property(e => e.SpanKind).HasMaxLength(16);
        builder.Property(e => e.ServiceName).IsRequired().HasMaxLength(64);

        builder.HasIndex(e => e.StartTime);
        builder.HasIndex(e => e.TraceId);
    }
}
