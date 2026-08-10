using EDMS1.CommandLog.Models;
using MediatR;
using WebApplicationExample.Database;
using WebApplicationExample.Models;

namespace WebApplicationExample.Commands;

public class AddOrUpdateTodoHandler(AppDbContext dbContext) : IRequestHandler<AddOrUpdateTodo, CommandResult>
{
	public async Task<CommandResult> Handle(AddOrUpdateTodo request, CancellationToken cancel)
	{
		var todo = await dbContext.Todos.FindAsync([request.Id], cancel);

		if (todo is null)
		{
			dbContext.Todos.Add(new Todo(request.Id, request.Title, request.Description, request.IsCompleted));
		}
		else
		{
			todo.UpdateTitle(request.Title);
			todo.UpdateDescription(request.Description);
			todo.SetCompleted(request.IsCompleted);
		}

		await dbContext.SaveChangesAsync(cancel);
		return CommandResult.Successful;
	}
}