using EDMS1.CommandLog.Extensions;
using Microsoft.EntityFrameworkCore;
using WebApplicationExample.Models;

namespace WebApplicationExample.Database;

public class AppDbContext : DbContext
{
    public DbSet<Todo> Todos { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.UseCommandLog();
    }
}