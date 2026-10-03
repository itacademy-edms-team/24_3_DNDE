using Ardalis.Specification;

namespace FinanceTrack.Finance.Infrastructure.Data;

// inherit from Ardalis.Specification type
public class EfRepository<T>(AppDbContext dbContext)
    : RepositoryBase<T>(dbContext),
        IReadRepository<T>,
        IRepository<T>
    where T : class, IAggregateRoot
{
    public override Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbContext.Set<T>().Add(entity);
        return Task.FromResult(entity);
    }

    public override Task<IEnumerable<T>> AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default
    )
    {
        DbContext.Set<T>().AddRange(entities);
        return Task.FromResult(entities);
    }

    public override Task<int> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbContext.Set<T>().Update(entity);
        return Task.FromResult(1);
    }

    public override Task<int> UpdateRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default
    )
    {
        DbContext.Set<T>().UpdateRange(entities);
        return Task.FromResult(1);
    }

    public override Task<int> DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        DbContext.Set<T>().Remove(entity);
        return Task.FromResult(1);
    }

    public override Task<int> DeleteRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default
    )
    {
        DbContext.Set<T>().RemoveRange(entities);
        return Task.FromResult(1);
    }

    public override Task<int> DeleteRangeAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default
    )
    {
        var query = ApplySpecification(specification);
        DbContext.Set<T>().RemoveRange(query);
        return Task.FromResult(1);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("UnitOfWork behavior expected");
    }
}
