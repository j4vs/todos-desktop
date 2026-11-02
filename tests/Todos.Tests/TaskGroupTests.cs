using Todos.Domain;
using Todos.Domain.Models;
using Todos.Infrastructure.Persistence;
using Todos.Presentation.ViewModels;

namespace Todos.Tests;

internal static class TaskGroupTests
{
    [Test]
    public static async Task GroupOrderAndRenameSurviveRestartWithoutChangingTasks()
    {
        var state = new AppState { TodoWindows = [new TodoWindowState()] };
        var list = state.TodoWindows[0];
        var now = DateTimeOffset.UtcNow;
        var a = TodoListOperations.AddGroup(list, "Backend", now);
        var b = TodoListOperations.AddGroup(list, "Frontend", now);
        var task = TodoListOperations.AddTask(list, "Conservar", now, a.Id);
        using (var vm = new TodoWindowViewModel(list, () => { }, () => { }, () => { }))
        {
            var group = vm.Groups.Single(item => item.Id == b.Id);
            vm.MoveGroup(group, -1);
            vm.MoveGroup(group, -1); // Boundary is a no-op.
            vm.MoveGroup(vm.Groups[0], 1); // General is fixed.
            Assert.SequenceEqual(new Guid?[] { null, b.Id, a.Id }, vm.Groups.Select(item => item.Id));
            group.Name = "Interfaz";
            vm.RenameGroup(group);
            group.Name = "Backend";
            vm.RenameGroup(group);
            Assert.Equal("Interfaz", group.Name);
            Assert.True(!string.IsNullOrEmpty(vm.GroupError));
        }
        using var directory = new PersistenceTests.TemporaryDirectory();
        var repository = new JsonAppStateRepository(new AppDataPaths(directory.Path));
        await repository.SaveAsync(state);
        var restored = (await repository.LoadAsync()).State.TodoWindows[0];
        Assert.SequenceEqual(new[] { b.Id, a.Id }, restored.Groups.Select(item => item.Id));
        Assert.Equal("Interfaz", restored.Groups[0].Name);
        Assert.Equal<Guid?>(a.Id, restored.Tasks.Single().GroupId);
        Assert.Equal(task.Id, restored.Tasks.Single().Id);
    }

    [Test]
    public static void ClearCompletedCanUndoWholeBatchAndPreservesPendingAndGroups()
    {
        var list = new TodoWindowState();
        var now = DateTimeOffset.UtcNow;
        var group = TodoListOperations.AddGroup(list, "Trabajo", now);
        var pending = TodoListOperations.AddTask(list, "Pendiente", now, group.Id);
        var a = TodoListOperations.AddTask(list, "Lista", now, group.Id);
        var b = TodoListOperations.AddTask(list, "Otra", now);
        TodoListOperations.CompleteTask(list, a.Id, now);
        TodoListOperations.CompleteTask(list, b.Id, now);
        using var vm = new TodoWindowViewModel(list, () => { }, () => { }, () => { });
        vm.ClearCompletedCommand.Execute(null);
        Assert.Equal(0, vm.CompletedCount);
        Assert.Equal(pending.Id, vm.PendingTasks.Single().Id);
        Assert.False(vm.ClearCompletedCommand.CanExecute(null));
        Assert.True(vm.IsUndoVisible);
        vm.UndoDeleteCommand.Execute(null);
        Assert.Equal(2, vm.CompletedCount);
        Assert.Equal<Guid?>(group.Id, list.Tasks.Single(item => item.Id == a.Id).GroupId);
        Assert.Equal<DateTimeOffset?>(now, list.Tasks.Single(item => item.Id == a.Id).CompletedAt);
        Assert.Equal(1, list.Groups.Count);
        Assert.False(vm.IsUndoVisible);
    }

    [Test]
    public static async Task AccordionStateSurvivesRefreshSnapshotAndRestart()
    {
        var state = new AppState { TodoWindows = [new TodoWindowState()] };
        var window = state.TodoWindows[0];
        using (var vm = new TodoWindowViewModel(window, () => { }, () => { }, () => { }))
        {
            vm.NewGroupName = "Plegable";
            vm.AddGroupCommand.Execute(null);
            vm.Groups[0].IsExpanded = false;
            vm.Groups[1].IsExpanded = false;
            vm.NewTaskText = "Conservar plegado al agregar";
            vm.AddTaskCommand.Execute(null);
            Assert.False(vm.Groups[1].IsExpanded);
            Assert.Equal(1, vm.Groups[1].Tasks.Count);
        }
        var snapshot = AppStateSnapshot.Clone(state);
        Assert.False(snapshot.TodoWindows[0].IsGeneralGroupExpanded);
        Assert.False(snapshot.TodoWindows[0].Groups[0].IsExpanded);
        using var directory = new PersistenceTests.TemporaryDirectory();
        var repository = new JsonAppStateRepository(new AppDataPaths(directory.Path));
        await repository.SaveAsync(snapshot);
        var restored = (await repository.LoadAsync()).State;
        using var reopened = new TodoWindowViewModel(restored.TodoWindows[0], () => { }, () => { }, () => { });
        Assert.False(reopened.Groups[0].IsExpanded);
        Assert.False(reopened.Groups[1].IsExpanded);
        Assert.Equal(1, reopened.Groups[1].Tasks.Count);
        Assert.True(new TodoWindowState().IsGeneralGroupExpanded);
        Assert.True(new TodoTaskGroup().IsExpanded);
    }

    [Test]
    public static async Task LegacyTasksMigrateAndGroupsRoundTripWithoutLosingData()
    {
        using var directory = new PersistenceTests.TemporaryDirectory();
        var paths = new AppDataPaths(directory.Path);
        Directory.CreateDirectory(directory.Path);
        await File.WriteAllTextAsync(paths.StateFile,
            """{"schemaVersion":1,"todoWindows":[{"title":"Anterior","tasks":[{"text":"Existente","pendingOrder":0}]}]}""");
        var repository = new JsonAppStateRepository(paths);
        var state = (await repository.LoadAsync()).State;
        var window = state.TodoWindows[0];
        Assert.Equal(AppConstants.CurrentSchemaVersion, state.SchemaVersion);
        Assert.Equal<Guid?>(null, window.Tasks[0].GroupId);
        var group = TodoListOperations.AddGroup(window, "Verificación", DateTimeOffset.UtcNow);
        TodoListOperations.MoveTaskToGroup(window, window.Tasks[0].Id, group.Id, 0, DateTimeOffset.UtcNow);
        await repository.SaveAsync(state);
        var restored = (await repository.LoadAsync()).State.TodoWindows[0];
        Assert.Equal("Verificación", restored.Groups[0].Name);
        Assert.Equal<Guid?>(group.Id, restored.Tasks[0].GroupId);
        Assert.Equal("Existente", restored.Tasks[0].Text);
    }

    [Test]
    public static void MovingAcrossGroupsPreservesEachOrderAndCompletedState()
    {
        var window = new TodoWindowState();
        var now = DateTimeOffset.UtcNow;
        var group = TodoListOperations.AddGroup(window, "Backend", now);
        var a = TodoListOperations.AddTask(window, "A", now);
        var b = TodoListOperations.AddTask(window, "B", now);
        var c = TodoListOperations.AddTask(window, "C", now, group.Id);
        TodoListOperations.MoveTaskToGroup(window, b.Id, group.Id, 0, now);
        Assert.SequenceEqual(new[] { b.Id, c.Id }, TodoListOperations.GetPending(window).Where(t => t.GroupId == group.Id).Select(t => t.Id));
        TodoListOperations.MovePendingTask(window, c.Id, 0, now);
        Assert.Equal(0, a.PendingOrder);
        Assert.Equal(0, c.PendingOrder);
        Assert.Equal(1, b.PendingOrder);
        TodoListOperations.CompleteTask(window, b.Id, now);
        TodoListOperations.MoveTaskToGroup(window, b.Id, null, 0, now);
        Assert.True(b.IsCompleted);
        Assert.Equal<DateTimeOffset?>(now, b.CompletedAt);
        TodoListOperations.ReopenTask(window, b.Id, now);
        Assert.SequenceEqual(new[] { a.Id, b.Id }, TodoListOperations.GetPending(window).Where(t => t.GroupId == null).Select(t => t.Id));
    }

    [Test]
    public static void RemovingGroupAndUndoingTaskDeletionNeverLoseTasks()
    {
        var window = new TodoWindowState();
        var now = DateTimeOffset.UtcNow;
        var group = TodoListOperations.AddGroup(window, "Pruebas", now);
        var a = TodoListOperations.AddTask(window, "General", now);
        var b = TodoListOperations.AddTask(window, "Pendiente", now, group.Id);
        var c = TodoListOperations.AddTask(window, "Terminada", now, group.Id);
        TodoListOperations.CompleteTask(window, c.Id, now);
        var removed = TodoListOperations.RemoveTask(window, b.Id, now);
        TodoListOperations.RemoveGroup(window, group.Id, now);
        TodoListOperations.RestoreTask(window, removed, now);
        Assert.Equal(3, window.Tasks.Count);
        Assert.True(window.Tasks.All(task => task.GroupId == null));
        Assert.True(c.IsCompleted);
        Assert.Equal(0, window.Groups.Count);
        Assert.Equal("General", a.Text);
    }

    [Test]
    public static void InvalidGroupsAreRejectedAndSnapshotsAreIndependent()
    {
        var window = new TodoWindowState();
        var now = DateTimeOffset.UtcNow;
        var group = TodoListOperations.AddGroup(window, "API", now);
        Assert.Throws<ArgumentException>(() => TodoListOperations.AddGroup(window, " general ", now));
        Assert.Throws<ArgumentException>(() => TodoListOperations.AddGroup(window, " api ", now));
        Assert.Throws<ArgumentException>(() => TodoListOperations.RenameGroup(window, group.Id, " ", now));
        var task = TodoListOperations.AddTask(window, "Uno", now, group.Id);
        Assert.Throws<ArgumentException>(() => TodoListOperations.MoveTaskToGroup(window, task.Id, Guid.NewGuid(), 0, now));
        var state = new AppState { TodoWindows = [window] };
        var clone = AppStateSnapshot.Clone(state);
        group.Name = "Otro";
        task.GroupId = Guid.NewGuid();
        Assert.Equal("API", clone.TodoWindows[0].Groups[0].Name);
        Assert.Equal<Guid?>(group.Id, clone.TodoWindows[0].Tasks[0].GroupId);
        AppStateNormalizer.Normalize(state);
        Assert.Equal<Guid?>(null, task.GroupId);
    }

    [Test]
    public static void ViewModelCreatesSelectsMovesAndRemovesGroups()
    {
        var state = new TodoWindowState();
        using var vm = new TodoWindowViewModel(state, () => { }, () => { }, () => { });
        Assert.Equal("General", vm.SelectedGroup!.Name);
        vm.NewTaskText = "Sin clasificar";
        vm.AddTaskCommand.Execute(null);
        vm.NewGroupName = "Frontend";
        vm.AddGroupCommand.Execute(null);
        Assert.Equal("Frontend", vm.SelectedGroup!.Name);
        vm.NewTaskText = "Vista";
        vm.AddTaskCommand.Execute(null);
        var group = vm.SelectedGroup;
        Assert.Equal(1, group.Tasks.Count);
        vm.MoveTaskToGroup(vm.Groups[0].Tasks[0].Id, group.Id, 0);
        Assert.Equal(0, vm.Groups[0].Tasks.Count);
        Assert.Equal("Sin clasificar", group.Tasks[0].Text);
        group.Name = "Interfaz";
        vm.RenameGroup(group);
        Assert.Equal("Interfaz", state.Groups[0].Name);
        vm.RemoveGroup(group);
        Assert.Equal(1, vm.Groups.Count);
        Assert.Equal(2, vm.Groups[0].Tasks.Count);
        Assert.True(vm.SelectedGroup!.IsGeneral);
    }
}
