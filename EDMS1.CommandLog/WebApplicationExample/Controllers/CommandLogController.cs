using EDMS1.CommandLog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationExample.Database;

namespace WebApplicationExample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommandLogController(CommandLogDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancel = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = dbContext.Set<CommandLogEntry>()
            .AsNoTracking()
            .OrderByDescending(x => x.CreationTime);

        var total = await query.CountAsync(cancel);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancel);

        return Ok(new
        {
            page,
            pageSize,
            total,
            items
        });
    }
}
