namespace Todos.Domain.Models;

public sealed class TodoTaskGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Nuevo grupo";
    public bool IsExpanded { get; set; } = true;
}
