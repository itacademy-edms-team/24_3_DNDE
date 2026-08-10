using Microsoft.Extensions.DependencyInjection;

namespace EDMS1.CommandLog.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IDisposable
{
	// per-test
	protected readonly TestWebApplicationFactory Factory = new();

	protected IServiceScope CreateScope()
	{
		return Factory.Services.CreateScope();
	}

	protected static CancellationToken Ct => TestContext.Current.CancellationToken;

	public void Dispose()
	{
		Factory.Dispose();
	}
}