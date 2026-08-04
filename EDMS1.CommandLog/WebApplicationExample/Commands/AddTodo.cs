using EDMS1.CommandLog.Commands;
using EDMS1.CommandLog.Models;
using MediatR;

namespace WebApplicationExample.Commands;

public class AddTodo : IRequest<CommandResult>, ICommand
{
    public Guid CorrelationToken { get; } = Guid.NewGuid();

    public Guid Id { get; init; } = Guid.NewGuid();

    public string Title { get; init; } = String.Empty;
    public string Description { get; init; } = String.Empty;
    public bool IsCompleted { get; init; }
}