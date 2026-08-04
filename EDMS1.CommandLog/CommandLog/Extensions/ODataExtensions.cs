using EDMS1.CommandLog.Models;
using Microsoft.OData.ModelBuilder;

namespace EDMS1.CommandLog.Extensions;

/// <summary>
/// Набор расширений для конфигурации CommandLog моделей в OData.
/// </summary>
public static class ODataExtensions
{
    /// <summary>
    /// Регистрирует набор сущностей <see cref="CommandLogEntry"/> (набор "CommandLog") в EDM-модели,
    /// чтобы отдавать журнал команд через OData-эндпоинт.
    /// </summary>
    /// <param name="modelBuilder">Строитель EDM-модели OData.</param>
    /// <returns>Тот же строитель модели для построения цепочки вызовов.</returns>
    public static ODataConventionModelBuilder AddCommandLogOData(this ODataConventionModelBuilder modelBuilder)
    {
        modelBuilder.EntitySet<CommandLogEntry>("CommandLog").EntityType.HasKey(x => x.CommandLogId);
        return modelBuilder;
    }
}