using HttpLogging.Abstractions;
using Microsoft.AspNetCore.Http;

namespace HttpLogging.Extractors;

internal sealed class RequestBodyExtractor : DataExtractor<HttpContext>
{
	protected override object? ExtractFrom(HttpContext http) =>
		HttpBodyParser.TryParseJson(http.Items[HttpLoggingKeys.RequestBody] as string);
}