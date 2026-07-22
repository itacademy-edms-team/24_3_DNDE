using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationExample.Commands;
using WebApplicationExample.Database;

namespace WebApplicationExample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController(IMediator mediator, AppDbContext dbContext) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancel)
    {
        var todo = await dbContext.Todos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancel);

        return todo is null ? NotFound() : Ok(todo);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancel = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = dbContext.Todos
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt);

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

    [HttpPost]
    public async Task<IActionResult> Add(AddTodo command, CancellationToken cancel)
    {
        await mediator.Send(command, cancel);

        return CreatedAtAction(nameof(GetById), new { id = command.Id }, new { command.Id });
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateTodo command, CancellationToken cancel)
    {
        var result = await mediator.Send(command, cancel);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancel)
    {
        var result = await mediator.Send(new DeleteTodo { Id = id }, cancel);

        return Ok(result);
    }
}
