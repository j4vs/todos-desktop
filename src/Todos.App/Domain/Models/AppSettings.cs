namespace Todos.Domain.Models;

public sealed class AppSettings
{
    public bool RunAtWindowsStartup { get; set; } = true;

    public bool HasSeenFirstRunHint { get; set; }
}
