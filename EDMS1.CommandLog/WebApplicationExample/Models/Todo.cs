namespace WebApplicationExample.Models;

public class Todo
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsCompleted { get; private set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private Todo() { }

    public Todo(Guid id, string title, string description, bool isCompleted)
    {
        Id = id;
        Title = title;
        Description = description;
        IsCompleted = isCompleted;
    }
    
    public Todo UpdateTitle(string title)
    {
        Title = title;
        Touch();
        return this;
    }

    public Todo UpdateDescription(string description)
    {
        Description = description;
        Touch();
        return this;
    }

    public Todo SetCompleted(bool isCompleted)
    {
        IsCompleted = isCompleted;
        Touch();
        return this;
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}