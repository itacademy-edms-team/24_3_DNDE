using FluentValidation;
using Media.Domain.Objects;

namespace Media.Application.Objects.Validation;

/// <summary>
/// Правила команды инициализации загрузки (ADR 0038): форма тела проверяется до домена,
/// чтобы в хранилище не уходили политики на заведомо неподходящие файлы.
/// </summary>
public sealed class InitUploadCommandValidator : AbstractValidator<InitUploadCommand>
{
    public InitUploadCommandValidator()
    {
        RuleFor(command => command.ContentType)
            .Must(contentType => MediaContentTypes.TryNormalize(contentType, out _))
            .WithErrorCode("Media.ContentTypeUnsupported");

        RuleFor(command => command.SizeBytes)
            .NotNull()
            .WithErrorCode("Media.FileTooLarge")
            .InclusiveBetween(MediaConstraints.MinUploadBytes, MediaConstraints.MaxUploadBytes)
            .WithErrorCode("Media.FileTooLarge");
    }
}
