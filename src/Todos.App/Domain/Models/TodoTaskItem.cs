namespace Todos.Domain.Models;

public sealed class TodoTaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Text { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public int PendingOrder { get; set; }

    // null is the permanent General group, including tasks from schema 1.
    public Guid? GroupId { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
