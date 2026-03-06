namespace FinanceTrack.Finance.Core.Shared;

public abstract class GuidEntityBase : EntityBase<Guid>
{
    protected GuidEntityBase()
    {
        if (Id == Guid.Empty)
        {
            Id = Guid.NewGuid();
        }
    }
}
