using HttpLogging.Abstractions;
using Microsoft.AspNetCore.Http;

namespace HttpLogging.Extractors;

internal sealed class RequestHeadersExtractor : DataExtractor<HttpContext>
{
	protected override object? ExtractFrom(HttpContext http) => http.Request.Headers;
}