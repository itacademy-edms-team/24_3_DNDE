using FastEndpoints;
using FinanceTrack.Finance.Infrastructure.Data.Config;
using FluentValidation;

namespace FinanceTrack.Finance.Web.Contributors;

/// <summary>
/// See: https://fast-endpoints.com/docs/validation
/// </summary>
public class CreateContributorValidator : Validator<CreateContributorRequest>
{
  public CreateContributorValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .WithMessage("Name is required.")
      .MinimumLength(2)
      .MaximumLength(ContributorDataSchemaConstants.DEFAULT_NAME_LENGTH);
  }
}
