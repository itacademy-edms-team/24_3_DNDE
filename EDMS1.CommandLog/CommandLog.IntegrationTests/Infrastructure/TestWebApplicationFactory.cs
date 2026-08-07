using EDMS1.CommandLog.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using WebApplicationExample.Database;

namespace EDMS1.CommandLog.IntegrationTests.Infrastructure;

/// <summary>
/// Кастомный <see cref="WebApplicationFactory{TEntryPoint}" /> для интеграционных тестов
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
	protected readonly string DbName = DbNameCreator.CreateDbName();

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureTestServices(services =>
		{
			services.RemoveAll<DbContextOptions<AppDbContext>>();
			services.RemoveAll<DbContextOptions>();
			services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(DbName));

			services.RemoveAll<IHostedService>();
		});
	}
}