using FinanceTrack.Finance.Core.CategoryAggregate;

namespace FinanceTrack.Finance.UnitTests.Core.CategoryAggregate;

public class CategoryTests
{
    private const string UserId = "user-1";

    [Fact]
    public void Create_SetsExpectedFields()
    {
        var category = Category.Create(UserId, "Salary", CategoryType.Income, "💰", "#00FF00");

        category.UserId.ShouldBe(UserId);
        category.Name.ShouldBe("Salary");
        category.Type.ShouldBe(CategoryType.Income);
        category.Icon.ShouldBe("💰");
        category.Color.ShouldBe("#00FF00");
    }

    [Fact]
    public void Create_WithoutOptionals_SetsNulls()
    {
        var category = Category.Create(UserId, "Food", CategoryType.Expense);

        category.Icon.ShouldBeNull();
        category.Color.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidUserId_Throws(string? badUserId)
    {
        Should.Throw<ArgumentException>(() =>
            Category.Create(badUserId!, "Name", CategoryType.Income));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidName_Throws(string? badName)
    {
        Should.Throw<ArgumentException>(() =>
            Category.Create(UserId, badName!, CategoryType.Income));
    }

    [Fact]
    public void UpdateName_SetsNewName()
    {
        var category = Category.Create(UserId, "Food", CategoryType.Expense);

        category.UpdateName("Groceries");

        category.Name.ShouldBe("Groceries");
    }

    [Fact]
    public void UpdateIcon_SetsNewIcon()
    {
        var category = Category.Create(UserId, "Food", CategoryType.Expense);

        category.UpdateIcon("🍕");

        category.Icon.ShouldBe("🍕");
    }

    [Fact]
    public void UpdateColor_SetsNewColor()
    {
        var category = Category.Create(UserId, "Food", CategoryType.Expense);

        category.UpdateColor("#FF0000");

        category.Color.ShouldBe("#FF0000");
    }
}
