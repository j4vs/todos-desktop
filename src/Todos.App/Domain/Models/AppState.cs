namespace Todos.Domain.Models;

public sealed class AppState
{
    public int SchemaVersion { get; set; } = AppConstants.CurrentSchemaVersion;

    public LauncherState LauncherState { get; set; } = new();

    public AppSettings Settings { get; set; } = new();

    public List<TodoWindowState> TodoWindows { get; set; } = [];

    public DateTimeOffset LastSavedAt { get; set; }
}
