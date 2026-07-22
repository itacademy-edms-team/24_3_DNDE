using EDMS1.CommandLog.Extensions;
using Microsoft.EntityFrameworkCore;

namespace WebApplicationExample.Database;

public class CommandLogDbContext : AppDbContext
{
    public CommandLogDbContext(DbContextOptions<CommandLogDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.UseCommandLog();
    }
}