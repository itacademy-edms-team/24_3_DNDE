using EDMS1.CommandLog.Exceptions;
using EDMS1.CommandLog.Models;
using MediatR;
using WebApplicationExample.Database;
using WebApplicationExample.Models;

namespace WebApplicationExample.Commands;

public class DeleteTodoHandler(AppDbContext dbContext) : IRequestHandler<DeleteTodo, CommandResult>
{
    public async Task<CommandResult> Handle(DeleteTodo request, CancellationToken cancel)
    {
        var todo = await dbContext.Todos.FindAsync([request.Id], cancellationToken: cancel) ?? throw new EntityNotFoundException(nameof(Todo), nameof(Todo.Id), request.Id);

        dbContext.Todos.Remove(todo);
        
        await dbContext.SaveChangesAsync(cancel);
        return CommandResult.Successful;
    }
}