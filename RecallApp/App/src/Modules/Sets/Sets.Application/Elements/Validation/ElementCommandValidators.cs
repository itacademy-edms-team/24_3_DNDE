using FluentValidation;

namespace Sets.Application.Elements.Validation;

/// <summary>
/// Предоставляет правила проверки команды создания элемента.
/// </summary>
/// <remarks>
/// Здесь только проверки формы запроса: наличие типа и содержимого.
/// Допустимость типа и схему содержимого проверяет домен, поэтому коды ошибок приходят из одного места (ADR 0038).
/// </remarks>
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

/// <summary>
/// Предоставляет правила проверки команды замены содержимого элемента.
/// </summary>
/// <remarks>
/// Проверяется только наличие содержимого.
/// Схему проверяет домен по текущему типу элемента.
/// </remarks>
public sealed class UpdateElementCommandValidator : AbstractValidator<UpdateElementCommand>
{
    public UpdateElementCommandValidator()
    {
        RuleFor(command => command.Payload)
            .NotNull()
            .WithErrorCode("Element.PayloadRequired");
    }
}
