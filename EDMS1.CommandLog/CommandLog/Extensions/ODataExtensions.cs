using EDMS1.CommandLog.Models;
using Microsoft.OData.ModelBuilder;

namespace EDMS1.CommandLog.Extensions;

public static class ODataExtensions
{
    public static ODataConventionModelBuilder AddCommandLogOData(this ODataConventionModelBuilder modelBuilder)
    {
        modelBuilder.EntitySet<CommandLogEntry>("CommandLog").EntityType.HasKey(x => x.CommandLogId);
        return modelBuilder;
    }
}