using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.SharedKernel;

namespace FinanceTrack.Finance.Benchmarks.Benchmarks;

public sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
{
    public Task DispatchAndClearEvents(IEnumerable<IHasDomainEvents> entitiesWithEvents) =>
        Task.CompletedTask;
}
