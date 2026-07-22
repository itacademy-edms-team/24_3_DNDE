using EDMS1.CommandLog.Commands;
using EDMS1.CommandLog.Models;
using MediatR;

namespace WebApplicationExample.Commands;

public class UpdateTodo : IRequest<CommandResult>, ICommand
{
    public Guid CorrelationToken { get; } = Guid.NewGuid();

    public Guid Id { get; init; }

    public string Title { get; init; } = null!;
    public string Description { get; init; } = null!;
}
