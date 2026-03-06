using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.Infrastructure.Data;

public sealed class EfUnitOfWork(AppDbContext appDbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        appDbContext.SaveChangesAsync(ct);
}
