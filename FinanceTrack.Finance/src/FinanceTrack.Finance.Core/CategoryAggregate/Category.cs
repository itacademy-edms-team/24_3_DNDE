using FinanceTrack.Finance.Core.Shared;

namespace FinanceTrack.Finance.Core.CategoryAggregate;

public sealed class Category : GuidEntityBase, IAggregateRoot
{
    public string UserId { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public CategoryType Type { get; private set; } = default!;
    public string? Icon { get; private set; }
    public string? Color { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    // ORM
    private Category() { }

    public static Category Create(
        string userId,
        string name,
        CategoryType type,
        string? icon = null,
        string? color = null
    )
    {
        Guard.Against.NullOrWhiteSpace(userId);
        Guard.Against.NullOrWhiteSpace(name);

        return new Category
        {
            UserId = userId,
            Name = name,
            Type = type,
            Icon = icon,
            Color = color,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public Category UpdateName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Name = name;
        return this;
    }

    public Category UpdateIcon(string? icon)
    {
        Icon = icon;
        return this;
    }

    public Category UpdateColor(string? color)
    {
        Color = color;
        return this;
    }
}
