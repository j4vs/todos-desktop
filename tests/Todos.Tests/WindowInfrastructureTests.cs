using Todos.Application;
using Todos.Application.Windows;

namespace Todos.Tests;

internal static class WindowInfrastructureTests
{
    [Test]
    public static void ExplicitTestFoldersUseIndependentInstances()
    {
        var first = Todos.Application.AppLaunchOptions.Parse(["--smoke-test", "--data-root", "C:\\Temp\\todos-first"]);
        var second = Todos.Application.AppLaunchOptions.Parse(["--smoke-test", "--data-root", "C:\\Temp\\todos-second"]);
        var render = Todos.Application.AppLaunchOptions.Parse(["--render-preview", "C:\\Temp\\preview.png", "--data-root", "C:\\Temp\\todos-first"]);
        Assert.True(first.InstanceName != second.InstanceName);
        Assert.True(first.InstanceName != "Todos");
        Assert.Equal(first.InstanceName, render.InstanceName);
        var bin = AppLaunchOptions.Parse(["--render-recycle-bin-preview", "C:\\Temp\\bin.png", "--data-root", "C:\\Temp\\todos-first"]);
        Assert.Equal(first.InstanceName, bin.InstanceName);
        Assert.Equal("Todos", Todos.Application.AppLaunchOptions.Parse([]).InstanceName);
        Assert.Equal("Todos", Todos.Application.AppLaunchOptions.Parse(["--autostart"]).InstanceName);
    }

    [Test]
    public static void LauncherPositionUsesSafeDefaultAndClampsEveryEdge()
    {
        var area = new DisplayArea(-1920, 0, 1920, 1040);
        var defaultPosition = WindowPositioning.EnsureVisible(null, null, 52, 52, area);
        Assert.Equal(-76d, defaultPosition.X);
        Assert.True(defaultPosition.Y >= 8);

        var topLeft = WindowPositioning.EnsureVisible(-5000, -5000, 52, 52, area);
        Assert.Equal(-1912d, topLeft.X);
        Assert.Equal(8d, topLeft.Y);

        var bottomRight = WindowPositioning.EnsureVisible(5000, 5000, 52, 52, area);
        Assert.Equal(-60d, bottomRight.X);
        Assert.Equal(980d, bottomRight.Y);
    }

    [Test]
    public static void LaunchOptionsRecognizeSmokeAutostartAndDataRoot()
    {
        var root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "todos-options");
        var preview = System.IO.Path.Combine(root, "preview.png");
        var launcherPreview = System.IO.Path.Combine(root, "launcher.png");
        var options = AppLaunchOptions.Parse([
            "--autostart", "--smoke-test", "--data-root", root, "--render-preview", preview,
            "--render-launcher-preview", launcherPreview, "--render-recycle-bin-preview", preview, "--preview-editing", "--smoke-delay", "4500",
        ]);
        Assert.True(options.IsAutoStart);
        Assert.True(options.IsSmokeTest);
        Assert.Equal(System.IO.Path.GetFullPath(root), options.DataRoot);
        Assert.Equal(System.IO.Path.GetFullPath(preview), options.RenderPreviewPath);
        Assert.Equal(System.IO.Path.GetFullPath(launcherPreview), options.RenderLauncherPreviewPath);
        Assert.Equal(System.IO.Path.GetFullPath(preview), options.RenderRecycleBinPreviewPath);
        Assert.True(options.RenderPreviewEditing);
        Assert.Equal(4500, options.SmokeDelayMilliseconds);
    }
}
