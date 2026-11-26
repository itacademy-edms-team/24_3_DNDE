using HttpLogging.Abstractions;
using Microsoft.AspNetCore.Http;

namespace HttpLogging.Extractors;

internal sealed class StartTimeExtractor : DataExtractor<HttpContext>
{
	protected override object? ExtractFrom(HttpContext http) => http.Items[HttpLoggingKeys.StartTime];
}