using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sets.Infrastructure.Data.Purge;

/// <summary>
/// Предоставляет фоновую чистку мягко удалённых наборов и элементов.
/// </summary>
/// <remarks>
/// <para>Первый проход выполняется сразу при старте,
/// затем - раз в <see cref="SetsPurgeOptions.Interval"/>.</para>
/// <para>За один проход удаляются строки, которые существуют позднее, чем <c>now - Retention</c>:
/// сначала наборы (элементы удаляются каскадом), затем элементы, чьи наборы остались живы.</para>
/// <para>Ошибка прохода логируется и не останавливает цикл: следующая попытка - по расписанию.</para>
/// <para>На каждый проход создаётся свой scope.</para>
/// </remarks>
internal sealed class SetsPurgeService(
    IServiceScopeFactory scopeFactory,
    IOptions<SetsPurgeOptions> options,
    TimeProvider clock,
    ILogger<SetsPurgeService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SetsDbContext>();

                var (sets, elements) = await SetsPurger.PurgeAsync(
                    context,
                    clock.GetUtcNow().UtcDateTime - settings.Retention,
                    settings.BatchSize,
                    stoppingToken);

                if (sets > 0 || elements > 0)
                    logger.LogInformation("Purged {Sets} sets and {Elements} elements past retention", sets, elements);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Soft-deleted content purge failed");
            }

            try
            {
                await Task.Delay(settings.Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
