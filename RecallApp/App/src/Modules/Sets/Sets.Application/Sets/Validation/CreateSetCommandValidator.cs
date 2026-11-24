using FluentValidation;
using Sets.Domain.Sets;

namespace Sets.Application.Sets.Validation;

/// <summary>
/// Предоставляет правила проверки команды создания набора.
/// </summary>
/// <remarks>
/// <para>Здесь только ранние проверки формы - длины полей: они дают 400 до обращения к домену.</para>
/// <para>Обязательность названия проверяет сам домен через <see cref="SetTitle.Create"/> -
/// код <c>Set.TitleRequired</c> приходит оттуда, а не из валидатора (ADR 0038).</para>
/// </remarks>
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
