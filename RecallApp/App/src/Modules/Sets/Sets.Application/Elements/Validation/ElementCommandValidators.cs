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

public sealed class ReorderElementsCommandValidator : AbstractValidator<ReorderElementsCommand>
{
    public ReorderElementsCommandValidator()
    {
        // Пустой список перестановкой не является; расхождение состава с БД проверяет обработчик через 409.
        RuleFor(command => command.ElementIds)
            .NotNull()
            .WithErrorCode("Element.OrderRequired");
    }
}
