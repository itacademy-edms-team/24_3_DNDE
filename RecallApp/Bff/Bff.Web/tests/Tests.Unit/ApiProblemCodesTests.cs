using System.Reflection;
using Host.Infrastructure;

namespace Tests.Unit;

/// <summary>
/// Коды отказов BFF.
/// </summary>
public class ApiProblemCodesTests
{
    [Fact]
    public void Codes_AllDeclared_UseSharedPascalCaseFormat()
    {
        var codes = typeof(ApiProblem)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f is { IsLiteral: true, FieldType: var t } && t == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToArray();

        Assert.NotEmpty(codes);
        Assert.All(codes, code => Assert.Matches("^[A-Z][A-Za-z0-9]*$", code));
        Assert.Equal(codes.Length, codes.Distinct().Count());
    }
}
