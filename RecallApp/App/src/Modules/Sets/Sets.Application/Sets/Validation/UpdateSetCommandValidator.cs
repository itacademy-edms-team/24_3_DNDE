using FluentValidation;
using Sets.Domain.Sets;

namespace Sets.Application.Sets.Validation;

/// <summary>Валидатор для команды полной замены полей набора (ADR 0038).</summary>
public sealed class UpdateSetCommandValidator : AbstractValidator<UpdateSetCommand>
{
    public UpdateSetCommandValidator()
    {
        RuleFor(command => command.Title)
            .MaximumLength(SetConstraints.TitleMaxLength)
            .WithErrorCode("Set.TitleTooLong");

        RuleFor(command => command.Description)
            .MaximumLength(SetConstraints.DescriptionMaxLength)
            .WithErrorCode("Set.DescriptionTooLong");
    }
}
