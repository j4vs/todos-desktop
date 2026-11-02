namespace Todos.Domain.Models;

public sealed class LauncherState
{
    public Guid? LastMinimizedWindowId { get; set; }

    public double? PositionX { get; set; }

    public double? PositionY { get; set; }

    public string? DisplayId { get; set; }
}
