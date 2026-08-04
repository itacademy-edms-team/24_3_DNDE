using EDMS1.CommandLog.Models;
using MediatR;
using WebApplicationExample.Database;
using WebApplicationExample.Models;

namespace WebApplicationExample.Commands;

public class AddTodoHandler (AppDbContext dbContext) : IRequestHandler<AddTodo, CommandResult>
{
    public async Task<CommandResult> Handle(AddTodo request, CancellationToken cancel)
    {
        dbContext.Todos.Add(new Todo(id: request.Id, title: request.Title, description: request.Description, isCompleted: request.IsCompleted));
        
        await dbContext.SaveChangesAsync(cancel);
        
        return CommandResult.Successful;
    }
}