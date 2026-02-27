using FinanceTrack.Finance.Core.ContributorAggregate;
using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.UseCases.Contributors.Create;

public class CreateContributorHandler(
    IRepository<Contributor> _repository,
    IUnitOfWork _unitOfWork
) : ICommandHandler<CreateContributorCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateContributorCommand request,
        CancellationToken cancellationToken
    )
    {
        var newContributor = new Contributor(request.Name);
        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            newContributor.SetPhoneNumber(request.PhoneNumber);
        }
        var createdItem = await _repository.AddAsync(newContributor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return createdItem.Id;
    }
}
