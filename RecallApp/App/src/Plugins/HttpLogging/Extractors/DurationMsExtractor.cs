using HttpLogging.Abstractions;
using Microsoft.AspNetCore.Http;

namespace HttpLogging.Extractors;

internal sealed class DurationMsExtractor : DataExtractor<HttpContext>
{
	protected override object? ExtractFrom(HttpContext http) => http.Items[HttpLoggingKeys.DurationMs];
}