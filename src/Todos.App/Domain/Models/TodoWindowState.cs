namespace Todos.Domain.Models;

public sealed class TodoWindowState
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = "Nueva lista";

    public TodoColorTheme ColorTheme { get; set; } = TodoColorTheme.Lavender;

    public double? PositionX { get; set; }

    public double? PositionY { get; set; }

    public double Width { get; set; } = AppConstants.DefaultWindowWidth;

    public double Height { get; set; } = AppConstants.DefaultWindowHeight;

    public bool IsVisible { get; set; } = true;

    public bool AreCompletedTasksExpanded { get; set; }

    public List<TodoTaskItem> Tasks { get; set; } = [];

    public List<TodoTaskGroup> Groups { get; set; } = [];
    public bool IsGeneralGroupExpanded { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
