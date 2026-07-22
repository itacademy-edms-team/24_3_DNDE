using EDMS1.CommandLog.Commands;
using EDMS1.CommandLog.Models;
using MediatR;

namespace WebApplicationExample.Commands;

public class DeleteTodo : IRequest<CommandResult>, ICommand
{
    public Guid CorrelationToken { get; } = Guid.NewGuid();
    
    public Guid Id { get; init; }
}