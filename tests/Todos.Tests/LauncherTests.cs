using Todos.Infrastructure.Windows;
using Todos.Presentation.ViewModels;
using Todos.Presentation.Windows;

namespace Todos.Tests;

internal static class LauncherTests
{
    [Test]
    public static void LauncherCommandsInvokeTheirAssignedActions()
    {
        var created = 0;
        var shown = 0;
        var hidden = 0;
        var exited = 0;
        var viewModel = new LauncherViewModel(
            true,
            () => created++,
            () => shown++,
            () => hidden++,
            _ => { },
            _ => { },
            _ => true,
            () =>
            {
                exited++;
                return Task.CompletedTask;
            });

        viewModel.CreateListCommand.Execute(null);
        viewModel.ShowAllCommand.Execute(null);
        viewModel.HideAllCommand.Execute(null);
        viewModel.ExitCommand.Execute(null);
        SpinWait.SpinUntil(() => exited == 1, TimeSpan.FromSeconds(1));

        Assert.Equal(1, created);
        Assert.Equal(1, shown);
        Assert.Equal(1, hidden);
        Assert.Equal(1, exited);
        Assert.True(viewModel.RunAtWindowsStartup);
    }

    [Test]
    public static void LauncherMenuTogglesWithoutChangingOtherState()
    {
        var viewModel = new LauncherViewModel(false, () => { }, () => { }, () => { }, _ => { }, _ => { }, _ => true, () => Task.CompletedTask);
        viewModel.ToggleMenu();
        Assert.True(viewModel.IsMenuOpen);
        viewModel.ToggleMenu();
        Assert.False(viewModel.IsMenuOpen);
    }

    [Test]
    public static void StartupPreferenceRemainsUnchangedWhenWindowsRejectsIt()
    {
        var attempts = 0;
        var viewModel = new LauncherViewModel(
            true,
            () => { },
            () => { },
            () => { },
            _ => { },
            _ => { },
            _ =>
            {
                attempts++;
                return false;
            },
            () => Task.CompletedTask);

        viewModel.RunAtWindowsStartup = false;

        Assert.Equal(1, attempts);
        Assert.True(viewModel.RunAtWindowsStartup);
    }

    [Test]
    public static void NotificationIconLifecycleDoesNotRequireAWindow()
    {
        using var service = new NotificationAreaService(() => { }, () => { }, () => { });
        Assert.NotNull(service);
    }

    [Test]
    public static void PopupActionsAreNotTreatedAsLauncherDragGestures()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var root = new System.Windows.Controls.Grid();
                var launcherSurface = new System.Windows.Controls.Border();
                var surfaceChild = new System.Windows.Controls.Image();
                var popupAction = new System.Windows.Controls.Button();
                launcherSurface.Child = surfaceChild;
                root.Children.Add(launcherSurface);
                root.Children.Add(popupAction);

                Assert.True(LauncherInputPolicy.IsInside(launcherSurface, launcherSurface));
                Assert.True(LauncherInputPolicy.IsInside(launcherSurface, surfaceChild));
                Assert.False(LauncherInputPolicy.IsInside(launcherSurface, popupAction));
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure is not null)
        {
            throw failure;
        }
    }
}
