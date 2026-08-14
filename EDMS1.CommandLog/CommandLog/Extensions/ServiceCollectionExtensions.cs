using EDMS1.CommandLog.BackgroundServices;
using EDMS1.CommandLog.Commands;
using EDMS1.CommandLog.Helpers;
using EDMS1.CommandLog.Models;
using EDMS1.CommandLog.Resolvers;
using EDMS1.CommandLog.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EDMS1.CommandLog.Extensions;

/// <summary>
/// Набор расширений для регистрации сервисов CommandLog в хосте.
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// <para>
	/// Регистрация логирующихся команд: сервис журнала, контекст команды,
	/// доступ к провайдеру и фоновый сервис повторной обработки.
	/// </para>
	/// <remarks>
	/// Пример регистрации CommandLogService взят из <see href="https://github.com/dotnet/eShop/blob/main/src/Catalog.API/Extensions/Extensions.cs">eShop</see>
	/// </remarks>
	/// </summary>
	/// <typeparam name="TContext">
	/// Тип <see cref="DbContext"/> хоста, в модель которого добавлена таблица журнала (через <c>UseCommandLog</c>).
	/// Через него сервис записывает команды журнала.
	/// </typeparam>
	/// <param name="services">Коллекция сервисов.</param>
	/// <returns>Та же коллекция сервисов для построения цепочки вызовов.</returns>
	public static IServiceCollection AddCommandLogService<TContext>(this IServiceCollection services)
		where TContext : DbContext
	{
		services.AddSingleton<CommandTypeResolver>();

		services.AddScoped<ICommandLogService, CommandLogService<TContext>>();

		services.AddScoped<ICommandContext, CommandContext>();

		services.AddHttpContextAccessor();
		services.AddSingleton<ServiceProviderAccessor>();
		services.AddHostedService<CommandLogRetryBackgroundService>();

		return services;
	}
}