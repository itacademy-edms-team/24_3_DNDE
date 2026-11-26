using HttpLogging.Abstractions;
using Microsoft.AspNetCore.Http;

namespace HttpLogging.Extractors;

internal sealed class StatusCodeExtractor : DataExtractor<HttpContext>
{
	protected override object? ExtractFrom(HttpContext http) => http.Items[HttpLoggingKeys.StatusCode];
}