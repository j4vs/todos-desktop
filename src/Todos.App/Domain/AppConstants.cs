namespace Todos.Domain;

public static class AppConstants
{
    public const int CurrentSchemaVersion = 5;
    public const int MaxTitleLength = 80;
    public const int MaxTaskTextLength = 300;
    public const double DefaultWindowWidth = 410;
    public const double DefaultWindowHeight = 420;
    public const double MinimumWindowWidth = 280;
    public const double MinimumWindowHeight = 220;
    public const double LauncherSize = 52;
    public const double LauncherWidth = 52;
    public const double LauncherHeight = 52;
    public static readonly TimeSpan UndoDeleteDuration = TimeSpan.FromSeconds(5);
    public static readonly TimeSpan DefaultSaveDebounce = TimeSpan.FromMilliseconds(350);
}
