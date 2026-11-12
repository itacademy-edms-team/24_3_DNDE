using System.Reflection;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Common.Application.Behaviors;

/// <summary>
/// Правила команды/запроса через FluentValidation в конвейере MediatR (ADR 0038).
/// Ошибки возвращаются как <see cref="ErrorOr"/>-значения, исключений в штатном пути нет.
/// </summary>
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
    /// Код ошибки задаётся в валидаторе через WithErrorCode и обязан быть контрактным
    /// (<c>&lt;Сущность&gt;.&lt;Правило&gt;</c>); если его забыли — отдаём безопасный общий код.
    /// </summary>
    private static Error ToError(FluentValidation.Results.ValidationFailure failure)
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

/// <summary>Коды ошибок, которые формирует сам конвейер валидации.</summary>
public static class ValidationCodes
{
    /// <summary>Код по умолчанию, если валидатор не предоставил свой.</summary>
    public const string Default = "Validation.InvalidRequest";
}

/// <summary>
/// Создаёт ответ-ошибку для поведения MediatR. Поведение объявлено как
/// <c>ValidationBehavior&lt;TRequest, TResponse&gt;</c>, где <typeparamref name="TResponse"/>
/// известен только как <see cref="IErrorOr"/>, поэтому вызвать
/// <c>ErrorOr&lt;T&gt;.From(errors)</c> из него напрямую нельзя: компилятор не знает T.
/// <para>
/// Решение: у типа ответа ищется статический метод <c>From(List&lt;Error&gt;)</c>
/// (он есть у каждого закрытого <c>ErrorOr&lt;T&gt;</c>), из него строится делегат и
/// сохраняется в статическом поле закрытого генерика. CLR инициализирует это поле
/// один раз на тип ответа, поэтому рефлексия выполняется при первом запросе такого типа,
/// а все последующие вызовы идут через обычный делегат.
/// </para>
/// Если тип ответа реализует <see cref="IErrorOr"/>, но не имеет такого метода
/// (например, свой result-тип), конструктор бросит <see cref="InvalidOperationException"/>
/// при первом же вызове.
/// </summary>
internal static class ErrorResultFactory<TResponse>
    where TResponse : IErrorOr
{
    private static readonly Func<List<Error>, TResponse> FromErrors = CreateFromErrors();

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
