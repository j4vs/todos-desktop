using Todos.Infrastructure.Windows;
using Todos.Presentation.ViewModels;
using Todos.Presentation.Windows;

namespace Todos.Tests;

internal static class LauncherTests
{
    [Test]
    public static void TrayVisibilityActionChangesLabelAndKeepsCreationAndExit()
    {
        var visible = true;
        var created = 0;
        var exited = 0;
        using var service = new NotificationAreaService(() => visible = true, () => created++, () => exited++,
            () => visible = !visible, () => visible);
        var icon = (System.Windows.Forms.NotifyIcon)typeof(NotificationAreaService)
            .GetField("_notifyIcon", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(service)!;
        var menu = icon.ContextMenuStrip!;
        Assert.Equal("Ocultar botón", menu.Items[0].Text);
        menu.Items[0].PerformClick();
        service.UpdateLauncherVisibility(visible);
        Assert.False(visible);
        Assert.Equal("Mostrar botón", menu.Items[0].Text);
        menu.Items[1].PerformClick();
        Assert.Equal(1, created);
        Assert.False(visible);
        menu.Items[0].PerformClick();
        service.UpdateLauncherVisibility(visible);
        Assert.True(visible);
        Assert.Equal("Ocultar botón", menu.Items[0].Text);
        Assert.False(menu.Items.Cast<System.Windows.Forms.ToolStripItem>().Any(item => item.Text == "Mostrar Todo's"));
        menu.Items[3].PerformClick();
        Assert.Equal(1, exited);
    }

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
    [Test]
    public static void PrimaryClickRecoversAssociatedWindowAndDeletionDoesNotChooseAnother()
    {
        Guid? shown = null;
        var vm = new LauncherViewModel(false, () => { }, () => { }, () => { }, id => shown = id, _ => { }, _ => true, () => Task.CompletedTask);
        var first = new Todos.Domain.Models.TodoWindowState { Title = "API", IsVisible = false, ColorTheme = Todos.Domain.Models.TodoColorTheme.Rose };
        var other = new Todos.Domain.Models.TodoWindowState { Title = "Frontend", IsVisible = false };
        vm.RefreshHiddenWindows([first, other]);
        vm.RefreshAssociation([first, other], first.Id);
        vm.PrimaryClick();
        Assert.Equal(first.Id, shown);
        Assert.Equal("API", vm.LauncherTitle);
        Assert.Equal(Todos.Presentation.Themes.TodoThemePalette.GetLauncherBackground(first.ColorTheme), vm.LauncherColor);
        first.IsVisible = true;
        vm.RefreshAssociation([first, other], first.Id);
        vm.PrimaryClick();
        Assert.Equal(first.Id, shown);
        vm.RefreshHiddenWindows([other]);
        vm.RefreshAssociation([other], first.Id);
        Assert.Equal<Guid?>(null, vm.AssociatedWindowId);
        Assert.Equal("#9285D8", vm.LauncherColor);
        shown = null;
        vm.PrimaryClick();
        Assert.Equal<Guid?>(null, shown);
        Assert.True(vm.IsHiddenSubmenuOpen);
    }

    [Test]
    public static void RightClickActionsReplaceHiddenListsAndWinOverHover()
    {
        var vm = new LauncherViewModel(false, () => { }, () => { }, () => { }, _ => { }, _ => { }, _ => true, () => Task.CompletedTask);
        vm.RefreshHiddenWindows([new Todos.Domain.Models.TodoWindowState { IsVisible = false }]);
        vm.ShowHiddenLists();
        Assert.True(vm.IsHiddenSubmenuOpen);
        vm.ToggleMenu();
        Assert.True(vm.IsMenuOpen);
        Assert.False(vm.IsHiddenSubmenuOpen);
        vm.ShowHiddenLists();
        Assert.True(vm.IsMenuOpen);
        Assert.False(vm.IsHiddenSubmenuOpen);
        vm.IsSettingsOpen = true;
        vm.ClosePanels();
        Assert.False(vm.IsPanelOpen);
        Assert.False(vm.IsSettingsOpen);
        vm.PrimaryClick();
        Assert.True(vm.IsHiddenSubmenuOpen);
    }

    [Test]
    public static void PressSnapshotClosesVisibleAssociatedListWithoutReopeningAfterOutsideHide()
    {
        var state = new Todos.Domain.Models.TodoWindowState { IsVisible = true };
        var opened = 0;
        var closed = 0;
        var vm = new LauncherViewModel(false, () => { }, () => { }, () => { }, _ => { opened++; state.IsVisible = true; },
            _ => { }, _ => true, () => Task.CompletedTask, true, null, _ => { closed++; state.IsVisible = false; });
        vm.RefreshAssociation([state], state.Id);
        var finishClick = vm.PreparePrimaryClick();
        state.IsVisible = false; // The global outside-click handler runs before mouse-up.
        vm.RefreshAssociation([state], state.Id);
        finishClick();
        Assert.Equal(0, opened);
        Assert.Equal(1, closed);
        Assert.False(state.IsVisible);
        vm.PrimaryClick();
        Assert.Equal(1, opened);
        Assert.True(state.IsVisible);
    }

}
