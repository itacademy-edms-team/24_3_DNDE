namespace Host.Infrastructure;

/// <summary>
/// Происхождение ошибки в problem-ответах (extension "source").
/// Чтобы различать источник ошибки, какой сервис породил.
/// </summary>
public static class ProblemSource
{
    public const string Bff = "bff";
}
