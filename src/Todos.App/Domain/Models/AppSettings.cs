namespace Todos.Domain.Models;

public sealed class AppSettings
{
    public int FontSizeStep { get; set; }
    public int LauncherSize { get; set; } = 1;
    public int SpacingSize { get; set; } = 1;

    public bool AutoMinimizeWindows { get; set; }

    public bool RunAtWindowsStartup { get; set; } = true;

    public bool HasSeenFirstRunHint { get; set; }
}
