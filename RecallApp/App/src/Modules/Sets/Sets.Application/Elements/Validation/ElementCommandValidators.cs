using FluentValidation;

namespace Sets.Application.Elements.Validation;

public sealed class CreateElementCommandValidator : AbstractValidator<CreateElementCommand>
{
    public CreateElementCommandValidator()
    {
        RuleFor(command => command.Type)
            .NotEmpty()
            .WithErrorCode("Element.TypeRequired");

        RuleFor(command => command.Payload)
            .NotNull()
            .WithErrorCode("Element.PayloadRequired");
    }
}

public sealed class UpdateElementCommandValidator : AbstractValidator<UpdateElementCommand>
{
    public UpdateElementCommandValidator()
    {
        RuleFor(command => command.Payload)
            .NotNull()
            .WithErrorCode("Element.PayloadRequired");
    }
}
