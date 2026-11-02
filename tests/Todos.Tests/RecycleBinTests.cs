using Todos.Domain;
using Todos.Domain.Models;
using Todos.Infrastructure.Persistence;
using Todos.Presentation.ViewModels;

namespace Todos.Tests;

internal static class RecycleBinTests
{
    [Test]
    public static async Task TrashSurvivesRestartAndRestoresTheCompleteList()
    {
        using var directory = new PersistenceTests.TemporaryDirectory();
        var repository = new JsonAppStateRepository(new AppDataPaths(directory.Path));
        var window = new TodoWindowState { Title = "Lista ñ", ColorTheme = TodoColorTheme.Sky, IsGeneralGroupExpanded = false,
            Width = 560, Height = 620, PositionX = 120, PositionY = 80, AreCompletedTasksExpanded = true };
        var now = DateTimeOffset.UtcNow;
        var group = TodoListOperations.AddGroup(window, "Backend", now);
        group.IsExpanded = false;
        var pending = TodoListOperations.AddTask(window, "Pendiente", now, group.Id);
        var completed = TodoListOperations.AddTask(window, "Terminada", now);
        TodoListOperations.CompleteTask(window, completed.Id, now);
        var state = new AppState { TodoWindows = [window] };
        state.LauncherState.LastMinimizedWindowId = window.Id;
        Assert.True(RecycleBinOperations.MoveToTrash(state, window.Id, now));
        Assert.Equal<Guid?>(null, state.LauncherState.LastMinimizedWindowId);
        Assert.Equal(0, state.TodoWindows.Count);
        Assert.False(window.IsVisible);
        var snapshot = AppStateSnapshot.Clone(state);
        window.Tasks[0].Text = "Cambio posterior al snapshot";
        group.Name = "Otro";
        await repository.SaveAsync(snapshot);
        var loaded = (await repository.LoadAsync()).State;
        Assert.Equal(1, loaded.TrashedWindows.Count);
        Assert.False(loaded.TrashedWindows[0].IsVisible);
        Assert.Equal<DateTimeOffset?>(now, loaded.TrashedWindows[0].TrashedAt);
        var restored = RecycleBinOperations.Restore(loaded, window.Id)!;
        Assert.Equal(0, loaded.TrashedWindows.Count);
        Assert.Equal(1, loaded.TodoWindows.Count);
        Assert.Equal(window.Id, restored.Id);
        Assert.Equal("Lista ñ", restored.Title);
        Assert.Equal(TodoColorTheme.Sky, restored.ColorTheme);
        Assert.Equal(560d, restored.Width);
        Assert.Equal(620d, restored.Height);
        Assert.Equal(120d, restored.PositionX!.Value);
        Assert.Equal("Backend", restored.Groups[0].Name);
        Assert.False(restored.Groups[0].IsExpanded);
        Assert.False(restored.IsGeneralGroupExpanded);
        Assert.True(restored.AreCompletedTasksExpanded);
        Assert.Equal(pending.Id, restored.Tasks.Single(task => !task.IsCompleted).Id);
        Assert.Equal("Pendiente", restored.Tasks.Single(task => !task.IsCompleted).Text);
        Assert.Equal<Guid?>(group.Id, restored.Tasks.Single(task => !task.IsCompleted).GroupId);
        Assert.Equal(completed.Id, restored.Tasks.Single(task => task.IsCompleted).Id);
        Assert.Equal<DateTimeOffset?>(null, restored.TrashedAt);
        Assert.Equal<Guid?>(null, loaded.LauncherState.LastMinimizedWindowId);
        Assert.True(restored.IsVisible);
        await repository.SaveAsync(loaded);
        var restarted = (await repository.LoadAsync()).State;
        Assert.Equal(0, restarted.TrashedWindows.Count);
        Assert.Equal(2, restarted.TodoWindows[0].Tasks.Count);
    }

    [Test]
    public static void LegacyStateAndDamagedTrashNormalizeWithoutLosingLists()
    {
        var active = new TodoWindowState();
        var trash = new TodoWindowState { Id = active.Id, IsVisible = true };
        var state = new AppState { SchemaVersion = 2, TodoWindows = [active], TrashedWindows = [trash] };
        AppStateNormalizer.Normalize(state);
        Assert.Equal(AppConstants.CurrentSchemaVersion, state.SchemaVersion);
        Assert.Equal(active.Id, state.TodoWindows[0].Id);
        Assert.False(trash.Id == active.Id);
        Assert.False(trash.IsVisible);
        Assert.True(trash.TrashedAt.HasValue);
        var legacy = new AppState { SchemaVersion = 2, TrashedWindows = null! };
        Assert.Equal(0, AppStateNormalizer.Normalize(legacy).TrashedWindows.Count);
    }

    [Test]
    public static void PermanentDeletionRequiresConfirmationAndOnlyAffectsItsTrashedList()
    {
        var keep = new TodoWindowState { Title = "Conservar" };
        var remove = new TodoWindowState { Title = "Eliminar" };
        var active = new TodoWindowState();
        var state = new AppState { TodoWindows = [active], TrashedWindows = [keep, remove] };
        RecycleBinViewModel? vm = null;
        vm = new RecycleBinViewModel(_ => { }, id => { RecycleBinOperations.DeletePermanently(state, id); vm!.Refresh(state.TrashedWindows); });
        vm.Refresh(state.TrashedWindows);
        vm.Lists.Single(item => item.Id == remove.Id).DeleteCommand.Execute(null);
        Assert.True(vm.IsConfirmingDelete);
        Assert.Equal(2, state.TrashedWindows.Count);
        vm.CancelDeleteCommand.Execute(null);
        Assert.False(vm.IsConfirmingDelete);
        Assert.Equal(2, state.TrashedWindows.Count);
        vm.Lists.Single(item => item.Id == remove.Id).DeleteCommand.Execute(null);
        vm.ConfirmDeleteCommand.Execute(null);
        vm.ConfirmDeleteCommand.Execute(null);
        Assert.Equal(1, state.TrashedWindows.Count);
        Assert.Equal(keep.Id, state.TrashedWindows[0].Id);
        Assert.Equal(active.Id, state.TodoWindows[0].Id);
        Assert.False(vm.IsConfirmingDelete);
        Assert.False(RecycleBinOperations.DeletePermanently(state, active.Id));
    }

    [Test]
    public static void RepeatedMoveAndRestoreNeverDuplicateAList()
    {
        var window = new TodoWindowState();
        var state = new AppState { TodoWindows = [window] };
        Assert.True(RecycleBinOperations.MoveToTrash(state, window.Id, DateTimeOffset.UtcNow));
        Assert.False(RecycleBinOperations.MoveToTrash(state, window.Id, DateTimeOffset.UtcNow));
        Assert.Equal(1, state.TrashedWindows.Count);
        Assert.True(RecycleBinOperations.Restore(state, window.Id) is not null);
        Assert.True(RecycleBinOperations.Restore(state, window.Id) is null);
        Assert.Equal(1, state.TodoWindows.Count);
        Assert.Equal(0, state.TrashedWindows.Count);
    }
}
