using HttpLogging.Abstractions;
using Microsoft.AspNetCore.Http;

namespace HttpLogging.Extractors;

internal sealed class ResponseBodyExtractor : DataExtractor<HttpContext>
{
	protected override object? ExtractFrom(HttpContext http) =>
		HttpBodyParser.TryParseJson(http.Items[HttpLoggingKeys.ResponseBody] as string);
}