namespace Todos.Application;

public sealed record AppLaunchOptions(
    bool IsSmokeTest,
    bool IsAutoStart,
    string? DataRoot,
    string? RenderPreviewPath,
    string? RenderLauncherPreviewPath,
    bool RenderPreviewEditing,
    int SmokeDelayMilliseconds)
{
    // Explicit test data must not activate an ordinary running instance.
    public string InstanceName => DataRoot is not null &&
        (IsSmokeTest || RenderPreviewPath is not null || RenderLauncherPreviewPath is not null)
        ? "Todos.Test." + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(Path.GetFullPath(DataRoot).ToUpperInvariant())))[..24]
        : "Todos";

    public static AppLaunchOptions Parse(IReadOnlyList<string> arguments)
    {
        var isSmokeTest = arguments.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase);
        var isAutoStart = arguments.Contains("--autostart", StringComparer.OrdinalIgnoreCase);
        var renderPreviewEditing = arguments.Contains("--preview-editing", StringComparer.OrdinalIgnoreCase);
        string? dataRoot = null;
        string? renderPreviewPath = null;
        string? renderLauncherPreviewPath = null;
        var smokeDelayMilliseconds = 900;

        for (var index = 0; index < arguments.Count - 1; index++)
        {
            if (string.Equals(arguments[index], "--data-root", StringComparison.OrdinalIgnoreCase))
            {
                dataRoot = Path.GetFullPath(arguments[index + 1]);
            }
            else if (string.Equals(arguments[index], "--render-preview", StringComparison.OrdinalIgnoreCase))
            {
                renderPreviewPath = Path.GetFullPath(arguments[index + 1]);
            }
            else if (string.Equals(arguments[index], "--render-launcher-preview", StringComparison.OrdinalIgnoreCase))
            {
                renderLauncherPreviewPath = Path.GetFullPath(arguments[index + 1]);
            }
            else if (string.Equals(arguments[index], "--smoke-delay", StringComparison.OrdinalIgnoreCase) &&
                     int.TryParse(arguments[index + 1], out var requestedDelay))
            {
                smokeDelayMilliseconds = Math.Clamp(requestedDelay, 100, 120_000);
            }
        }

        return new AppLaunchOptions(
            isSmokeTest,
            isAutoStart,
            dataRoot,
            renderPreviewPath,
            renderLauncherPreviewPath,
            renderPreviewEditing,
            smokeDelayMilliseconds);
    }
}
