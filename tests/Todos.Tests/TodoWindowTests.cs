using Todos.Domain;
using Todos.Domain.Models;
using Todos.Presentation.Themes;
using Todos.Presentation.ViewModels;
using Todos.Presentation.Windows;

namespace Todos.Tests;

internal static class TodoWindowTests
{
    [Test]
    public static void ReorderPolicyUsesTheVisibleInsertionSide()
    {
        Assert.Equal(0, TaskReorderPolicy.GetDestinationIndex(2, 0, false, 4));
        Assert.Equal(1, TaskReorderPolicy.GetDestinationIndex(3, 0, true, 4));
        Assert.Equal(2, TaskReorderPolicy.GetDestinationIndex(0, 2, true, 4));
        Assert.Equal(3, TaskReorderPolicy.GetDestinationIndex(0, 3, true, 4));
    }

    [Test]
    public static void DragVisualPolicyClonesFrozenTransformsBeforeAnimating()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var element = new System.Windows.Controls.Border();
                var frozenScale = new System.Windows.Media.ScaleTransform(0.35, 1);
                frozenScale.Freeze();
                element.RenderTransform = frozenScale;

                var mutableScale = TaskDragVisualPolicy.EnsureMutableScaleTransform(element);

                Assert.False(mutableScale.IsFrozen);
                Assert.False(ReferenceEquals(frozenScale, mutableScale));
                mutableScale.BeginAnimation(
                    System.Windows.Media.ScaleTransform.ScaleXProperty,
                    new System.Windows.Media.Animation.DoubleAnimation(1, TimeSpan.FromMilliseconds(1)));

                var frozenTranslation = new System.Windows.Media.TranslateTransform();
                frozenTranslation.Freeze();
                element.RenderTransform = frozenTranslation;
                var mutableTranslation = TaskDragVisualPolicy.EnsureMutableTranslateTransform(element);
                Assert.False(mutableTranslation.IsFrozen);
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
    public static void WindowViewModelCommitsTitleThemeBoundsAndActions()
    {
        var changed = 0;
        var hidden = 0;
        var deleted = 0;
        var state = new TodoWindowState();
        var viewModel = new TodoWindowViewModel(
            state,
            () => changed++,
            () => hidden++,
            () => deleted++);

        viewModel.Title = "  Proyecto principal  ";
        viewModel.CommitTitle();
        viewModel.SelectThemeCommand.Execute(TodoColorTheme.Mint);
        viewModel.CommitBounds(-1200, 35, 100, 80);
        viewModel.HideCommand.Execute(null);
        viewModel.DeleteCommand.Execute(null);

        Assert.Equal("Proyecto principal", state.Title);
        Assert.Equal(TodoColorTheme.Mint, state.ColorTheme);
        Assert.Equal(-1200d, state.PositionX);
        Assert.Equal(AppConstants.MinimumWindowWidth, state.Width);
        Assert.Equal(AppConstants.MinimumWindowHeight, state.Height);
        Assert.True(changed >= 3);
        Assert.Equal(1, hidden);
        Assert.Equal(1, deleted);
    }

    [Test]
    public static void LauncherListsOnlyHiddenWindowsAndRestoresSelectedId()
    {
        Guid? restored = null;
        Guid? deleted = null;
        var viewModel = new LauncherViewModel(
            true,
            () => { },
            () => { },
            () => { },
            id => restored = id,
            id => deleted = id,
            _ => true,
            () => Task.CompletedTask);
        var visible = new TodoWindowState { Title = "Visible", IsVisible = true };
        var hidden = new TodoWindowState { Title = "Oculta", IsVisible = false };

        Assert.False(viewModel.ToggleHiddenSubmenuCommand.CanExecute(null));
        viewModel.RefreshHiddenWindows([visible, hidden]);

        Assert.True(viewModel.HasHiddenWindows);
        Assert.True(viewModel.ToggleHiddenSubmenuCommand.CanExecute(null));
        Assert.Equal(1, viewModel.HiddenWindows.Count);
        Assert.Equal("Oculta", viewModel.HiddenWindows[0].Title);
        viewModel.HiddenWindows[0].ShowCommand.Execute(null);
        Assert.Equal(hidden.Id, restored);
        viewModel.HiddenWindows[0].DeleteCommand.Execute(null);
        Assert.Equal(hidden.Id, deleted);
        viewModel.ToggleHiddenSubmenuCommand.Execute(null);
        Assert.True(viewModel.IsHiddenSubmenuOpen);
        viewModel.IsMenuOpen = true;
        viewModel.IsMenuOpen = false;
        Assert.False(viewModel.IsHiddenSubmenuOpen);
    }

    [Test]
    public static void EveryThemeHasACompletePalette()
    {
        foreach (var theme in Enum.GetValues<TodoColorTheme>())
        {
            var colors = TodoThemePalette.Get(theme);
            Assert.True(colors.Background.StartsWith('#'));
            Assert.True(colors.Surface.StartsWith('#'));
            Assert.True(colors.Accent.StartsWith('#'));
            Assert.True(colors.Text.StartsWith('#'));
        }
    }

    [Test]
    public static void EveryThemeMeetsNormalTextContrastOnItsMainSurfaces()
    {
        foreach (var theme in Enum.GetValues<TodoColorTheme>())
        {
            var colors = TodoThemePalette.Get(theme);
            var backgroundRatio = ColorContrast.Ratio(colors.Text, colors.Background);
            var surfaceRatio = ColorContrast.Ratio(colors.Text, colors.Surface);

            Assert.True(backgroundRatio >= 4.5, $"{theme} background contrast was {backgroundRatio:N2}.");
            Assert.True(surfaceRatio >= 4.5, $"{theme} surface contrast was {surfaceRatio:N2}.");
        }
    }

    [Test]
    public static void TaskViewModelSupportsTheCompleteUserFlow()
    {
        var changes = 0;
        var state = new TodoWindowState();
        using var viewModel = new TodoWindowViewModel(state, () => changes++, () => { }, () => { });

        foreach (var text in new[] { "Backend", "Frontend", "Pruebas" })
        {
            viewModel.NewTaskText = text;
            viewModel.AddTaskCommand.Execute(null);
        }

        Assert.Equal(3, viewModel.PendingCount);
        Assert.Equal(string.Empty, viewModel.NewTaskText);
        var testsTask = viewModel.PendingTasks[2];
        viewModel.MovePendingTask(testsTask.Id, 0);
        Assert.Equal("Pruebas", viewModel.PendingTasks[0].Text);

        var backend = viewModel.PendingTasks.Single(task => task.Text == "Backend");
        backend.IsCompleted = true;
        Assert.Equal(2, viewModel.PendingCount);
        Assert.Equal(1, viewModel.CompletedCount);
        Assert.Equal("Backend", viewModel.CompletedTasks[0].Text);

        viewModel.CompletedTasks[0].IsCompleted = false;
        Assert.Equal("Backend", viewModel.PendingTasks[^1].Text);

        var frontend = viewModel.PendingTasks.Single(task => task.Text == "Frontend");
        frontend.BeginEditCommand.Execute(null);
        frontend.EditText = "Frontend actualizado";
        Assert.True(frontend.SaveEdit());
        Assert.Equal("Frontend actualizado", frontend.Text);

        frontend.DeleteCommand.Execute(null);
        Assert.True(viewModel.IsUndoVisible);
        Assert.Equal(2, viewModel.PendingCount);
        viewModel.UndoDeleteCommand.Execute(null);
        Assert.False(viewModel.IsUndoVisible);
        Assert.Equal(3, viewModel.PendingCount);

        Assert.False(viewModel.AreCompletedTasksExpanded);
        viewModel.ToggleCompletedSectionCommand.Execute(null);
        Assert.True(viewModel.AreCompletedTasksExpanded);
        Assert.True(changes >= 10);
    }

    [Test]
    public static void EmptyTaskCannotBeAddedOrSavedDuringEdit()
    {
        var state = new TodoWindowState();
        using var viewModel = new TodoWindowViewModel(state, () => { }, () => { }, () => { });
        viewModel.NewTaskText = "   ";
        Assert.False(viewModel.AddTaskCommand.CanExecute(null));

        viewModel.NewTaskText = "Tarea válida";
        viewModel.AddTaskCommand.Execute(null);
        var task = viewModel.PendingTasks[0];
        task.BeginEditCommand.Execute(null);
        task.EditText = "  ";
        Assert.False(task.SaveEdit());
        Assert.True(task.IsEditing);
        task.CancelEdit();
        Assert.Equal("Tarea válida", task.Text);
    }
}
