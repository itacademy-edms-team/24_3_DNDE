using EDMS1.CommandLog.Exceptions;
using EDMS1.CommandLog.Models;
using MediatR;
using WebApplicationExample.Database;
using WebApplicationExample.Models;

namespace WebApplicationExample.Commands;

public class UpdateTodoHandler(AppDbContext dbContext) : IRequestHandler<UpdateTodo, CommandResult>
{
    public async Task<CommandResult> Handle(UpdateTodo request, CancellationToken cancel)
    {
        var todo = await dbContext.Todos.FindAsync([request.Id], cancellationToken: cancel)
            ?? throw new EntityNotFoundException(nameof(Todo), nameof(Todo.Id), request.Id);

        todo.UpdateTitle(request.Title);
        todo.UpdateDescription(request.Description);

        await dbContext.SaveChangesAsync(cancel);

        return CommandResult.Successful;
    }
}
