using System.Reflection;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Common.Application.Behaviors;

/// <summary>
/// Проверяет команды и запросы через FluentValidation в конвейере MediatR.
/// </summary>
/// <remarks>
/// Ошибки возвращаются как <see cref="ErrorOr"/>-значения:
/// исключений в штатном пути нет (ADR 0038).
/// </remarks>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IErrorOr
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = results
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        return ErrorResultFactory<TResponse>.From(failures.Select(ToError).ToList());
    }

    /// <summary>
    /// Код задаётся через <c>WithErrorCode</c> и обязан быть контрактным
    /// (<c>&lt;Сущность&gt;.&lt;Правило&gt;</c>). Без <c>WithErrorCode</c> FluentValidation подставляет
    /// имя валидатора (<c>NotNullValidator</c>, <c>PredicateValidator</c> и т.п.), которое оканчивается
    /// на <c>Validator</c>; такой код подменяем общим кодом, чтобы наружу не утекало имя
    /// внутреннего валидатора.
    /// </summary>
    private static Error ToError(ValidationFailure failure)
    {
        var code = string.IsNullOrWhiteSpace(failure.ErrorCode) || failure.ErrorCode.EndsWith("Validator", StringComparison.Ordinal)
            ? ValidationCodes.Default
            : failure.ErrorCode;

        return Error.Validation(
            code: code,
            description: failure.ErrorMessage,
            metadata: new Dictionary<string, object> { ["target"] = ToTarget(code, failure.PropertyName) });
    }

    /// <summary>
    /// Target собирается из кода: <c>Set.TitleTooLong</c> → <c>set.title</c>.
    /// Так поле называется одинаково и для ошибок домена, и для ошибок валидатора (ADR 0038).
    /// </summary>
    private static string ToTarget(string code, string propertyName)
    {
        var property = string.IsNullOrEmpty(propertyName)
            ? "request"
            : char.ToLowerInvariant(propertyName[0]) + propertyName[1..];

        var separator = code.LastIndexOf('.');
        return separator <= 0
            ? property
            : $"{code[..separator].ToLowerInvariant()}.{property}";
    }
}

/// <summary>
/// Предоставляет код-заглушку для ошибок валидации, которым валидатор не задал контрактный код.
/// </summary>
public static class ValidationCodes
{
    /// <summary>Код по умолчанию, если валидатор не предоставил свой.</summary>
    public const string Default = "Validation.InvalidRequest";
}

/// <summary>
/// Создаёт ответ-ошибку для поведения MediatR.
/// </summary>
/// <remarks>
/// Поведение знает ответ только как <see cref="IErrorOr"/>: конкретный тип ошибки на этапе
/// компиляции неизвестен, поэтому статический метод построения ошибки вызывается рефлексией.
/// Метод ищется один раз на тип ответа и кешируется как делегат. Если у типа ответа такого метода
/// нет, выбрасывается <see cref="InvalidOperationException"/>.
/// </remarks>
internal static class ErrorResultFactory<TResponse>
    where TResponse : IErrorOr
{
    private static readonly Func<List<Error>, TResponse> FromErrors = CreateFromErrors();

    /// <summary>
    /// Возвращает ответ-ошибку по списку ошибок.
    /// </summary>
    /// <param name="errors">Ошибки валидации.</param>
    /// <returns>Ответ-ошибка.</returns>
    public static TResponse From(List<Error> errors) => FromErrors(errors);

    private static Func<List<Error>, TResponse> CreateFromErrors()
    {
        var fromErrors = typeof(TResponse).GetMethod(
            name: "From",
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: [typeof(List<Error>)],
            modifiers: null)
            ?? throw new InvalidOperationException(
                $"{typeof(TResponse).FullName} doesn't expose static From(List<Error>): " +
                "ValidationBehavior supports ErrorOr responses only (ADR 0038).");

        return fromErrors.CreateDelegate<Func<List<Error>, TResponse>>();
    }
}
