using FinanceTrack.Finance.Core.ContributorAggregate;
using FinanceTrack.Finance.Core.Interfaces;

namespace FinanceTrack.Finance.UseCases.Contributors.Update;

public class UpdateContributorHandler(
    IRepository<Contributor> _repository,
    IUnitOfWork _unitOfWork
) : ICommandHandler<UpdateContributorCommand, Result<ContributorDTO>>
{
    public async Task<Result<ContributorDTO>> Handle(
        UpdateContributorCommand request,
        CancellationToken cancellationToken
    )
    {
        var existingContributor = await _repository.GetByIdAsync(
            request.ContributorId,
            cancellationToken
        );
        if (existingContributor == null)
        {
            return Result.NotFound();
        }

        existingContributor.UpdateName(request.NewName!);

        await _repository.UpdateAsync(existingContributor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ContributorDTO(
            existingContributor.Id,
            existingContributor.Name,
            existingContributor.PhoneNumber?.Number ?? ""
        );
    }
}
