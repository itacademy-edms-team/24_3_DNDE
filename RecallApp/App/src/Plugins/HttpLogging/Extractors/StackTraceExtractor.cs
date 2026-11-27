using HttpLogging.Abstractions;
using NLog;

namespace HttpLogging.Extractors;

internal sealed class StackTraceExtractor : DataExtractor<LogEventInfo>
{
	protected override object? ExtractFrom(LogEventInfo logEvent) => logEvent.Exception?.ToString();
}