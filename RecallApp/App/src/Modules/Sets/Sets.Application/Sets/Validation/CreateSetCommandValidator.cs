using FluentValidation;
using Sets.Domain.Sets;

namespace Sets.Application.Sets.Validation;

/// <summary>
/// Валидатор для команды создания набора.
/// </summary>
public sealed class CreateSetCommandValidator : AbstractValidator<CreateSetCommand>
{
    public CreateSetCommandValidator()
    {
        RuleFor(command => command.Title)
            .MaximumLength(SetConstraints.TitleMaxLength)
            .WithErrorCode("Set.TitleTooLong");

        RuleFor(command => command.Description)
            .MaximumLength(SetConstraints.DescriptionMaxLength)
            .WithErrorCode("Set.DescriptionTooLong");
    }
}
