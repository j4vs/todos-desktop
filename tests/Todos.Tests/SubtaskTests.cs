using Todos.Domain;
using Todos.Domain.Models;
using Todos.Infrastructure.Persistence;
using Todos.Presentation.ViewModels;

namespace Todos.Tests;

internal static class SubtaskTests
{
    [Test]
    public static void CompletionCascadesReopeningAndConversionMaintainOneLevel()
    {
        var window = new TodoWindowState(); var now = DateTimeOffset.UtcNow;
        var root = TodoListOperations.AddTask(window, "Implementar módulo", now);
        var first = SubtaskOperations.Add(window, root.Id, "API", now);
        var second = SubtaskOperations.Add(window, root.Id, "Vista", now);
        TodoListOperations.CompleteTask(window, first.Id, now);
        Assert.False(root.IsCompleted);
        Assert.Equal(1, TodoListOperations.GetPending(window).Count);
        TodoListOperations.CompleteTask(window, second.Id, now);
        Assert.True(root.IsCompleted);
        Assert.Equal(1, TodoListOperations.GetCompleted(window).Count);
        TodoListOperations.ReopenTask(window, first.Id, now);
        Assert.False(root.IsCompleted); Assert.True(second.IsCompleted);
        TodoListOperations.CompleteTask(window, root.Id, now);
        Assert.True(root.Subtasks.All(child => child.IsCompleted));
        TodoListOperations.ReopenTask(window, root.Id, now);
        Assert.True(root.Subtasks.All(child => !child.IsCompleted));
        var source = TodoListOperations.AddTask(window, "Pruebas", now);
        SubtaskOperations.Convert(window, source.Id, root.Id, now);
        Assert.Equal(1, window.Tasks.Count);
        Assert.Equal(source.Id, root.Subtasks[2].Id);
        SubtaskOperations.Move(window, source.Id, 0, now);
        Assert.Equal(source.Id, root.Subtasks[0].Id);
        TodoListOperations.EditTask(window, source.Id, "Pruebas reales", now);
        Assert.Equal("Pruebas reales", root.Subtasks[0].Text);
    }

    [Test]
    public static void ConversionRejectsCyclesNestedTasksAndOtherGroupsBeforeMutation()
    {
        var window = new TodoWindowState(); var now = DateTimeOffset.UtcNow;
        var root = TodoListOperations.AddTask(window, "A", now);
        SubtaskOperations.Add(window, root.Id, "Paso", now);
        var group = TodoListOperations.AddGroup(window, "Otro", now);
        var target = TodoListOperations.AddTask(window, "B", now);
        var outside = TodoListOperations.AddTask(window, "C", now, group.Id);
        Assert.Throws<ArgumentException>(() => SubtaskOperations.Convert(window, target.Id, target.Id, now));
        Assert.Throws<ArgumentException>(() => SubtaskOperations.Convert(window, root.Id, target.Id, now));
        Assert.Throws<ArgumentException>(() => SubtaskOperations.Convert(window, target.Id, outside.Id, now));
        Assert.Equal(3, window.Tasks.Count);
        Assert.Equal(0, target.Subtasks.Count);
    }

    [Test]
    public static void CompletionUndoRestoresMixedProgressWithoutOverwritingEditedText()
    {
        var window = new TodoWindowState(); var now = DateTimeOffset.UtcNow;
        var root = TodoListOperations.AddTask(window, "Módulo", now);
        var done = SubtaskOperations.Add(window, root.Id, "API", now);
        var pending = SubtaskOperations.Add(window, root.Id, "Vista", now);
        TodoListOperations.CompleteTask(window, done.Id, now);
        using var vm = new TodoWindowViewModel(window, () => { }, () => { }, () => { });
        vm.PendingTasks[0].IsCompleted = true;
        Assert.Equal(1, vm.CompletedCount);
        Assert.True(vm.IsUndoVisible);
        TodoListOperations.EditTask(window, pending.Id, "Vista editada", now);
        vm.UndoDeleteCommand.Execute(null);
        Assert.False(root.IsCompleted); Assert.True(done.IsCompleted); Assert.False(pending.IsCompleted);
        Assert.Equal("Vista editada", pending.Text);
        Assert.Equal("1/2", vm.PendingTasks[0].SubtaskProgress);
    }

    [Test]
    public static void ClearCompletedPreservesPartialStepsAndDeletionUndoRestoresTrees()
    {
        var window = new TodoWindowState(); var now = DateTimeOffset.UtcNow;
        var root = TodoListOperations.AddTask(window, "Pendiente", now);
        var child = SubtaskOperations.Add(window, root.Id, "Hecha", now);
        SubtaskOperations.Add(window, root.Id, "Falta", now);
        TodoListOperations.CompleteTask(window, child.Id, now);
        var done = TodoListOperations.AddTask(window, "Completa", now);
        var doneChild = SubtaskOperations.Add(window, done.Id, "Paso", now);
        TodoListOperations.CompleteTask(window, done.Id, now);
        using var vm = new TodoWindowViewModel(window, () => { }, () => { }, () => { });
        vm.ClearCompletedCommand.Execute(null);
        Assert.Equal(1, window.Tasks.Count); Assert.Equal(2, root.Subtasks.Count);
        vm.UndoDeleteCommand.Execute(null);
        Assert.Equal(doneChild.Id, window.Tasks.Single(task => task.Id == done.Id).Subtasks.Single().Id);
        vm.PendingTasks[0].DeleteCommand.Execute(null);
        Assert.False(window.Tasks.Contains(root));
        vm.NewTaskText = "Creada después"; vm.AddTaskCommand.Execute(null);
        vm.UndoDeleteCommand.Execute(null);
        Assert.True(window.Tasks.Contains(root)); Assert.Equal(3, window.Tasks.Count);
        var rootVm = vm.PendingTasks.Single(task => task.Id == root.Id);
        rootVm.Subtasks.Single(task => !task.IsCompleted).DeleteCommand.Execute(null);
        Assert.True(root.IsCompleted);
        vm.UndoDeleteCommand.Execute(null);
        Assert.False(root.IsCompleted); Assert.Equal(2, root.Subtasks.Count);
    }

    [Test]
    public static async Task SnapshotTrashAndRestartPreserveStepsOrderAndFolding()
    {
        var state = new AppState(); var now = DateTimeOffset.UtcNow;
        var window = new TodoWindowState(); state.TodoWindows.Add(window);
        var group = TodoListOperations.AddGroup(window, "Backend", now);
        var root = TodoListOperations.AddTask(window, "Entrega", now, group.Id);
        var child = SubtaskOperations.Add(window, root.Id, "Uno", now);
        var second = SubtaskOperations.Add(window, root.Id, "Dos", now);
        SubtaskOperations.Move(window, second.Id, 0, now);
        root.AreSubtasksExpanded = false;
        var snapshot = AppStateSnapshot.Clone(state);
        root.Subtasks[0].Text = "Mutado";
        Assert.Equal("Dos", snapshot.TodoWindows[0].Tasks[0].Subtasks[0].Text);
        state = snapshot;
        RecycleBinOperations.MoveToTrash(state, window.Id, now);
        using var directory = new PersistenceTests.TemporaryDirectory();
        var repository = new JsonAppStateRepository(new AppDataPaths(directory.Path));
        await repository.SaveAsync(state);
        var loaded = (await repository.LoadAsync()).State;
        Assert.Equal(AppConstants.CurrentSchemaVersion, loaded.SchemaVersion);
        RecycleBinOperations.Restore(loaded, window.Id);
        var restored = loaded.TodoWindows.Single(); var parent = restored.Tasks.Single();
        Assert.False(parent.AreSubtasksExpanded);
        Assert.SequenceEqual(new[] { second.Id, child.Id }, parent.Subtasks.Select(task => task.Id));
        TodoListOperations.RemoveGroup(restored, group.Id, now);
        Assert.True(parent.Subtasks.All(task => task.GroupId is null));
        var temporary = TodoListOperations.AddGroup(restored, "Temporal", now);
        TodoListOperations.MoveTaskToGroup(restored, parent.Id, temporary.Id, 0, now);
        Assert.True(parent.Subtasks.All(task => task.GroupId == temporary.Id));
        var removed = TodoListOperations.RemoveTask(restored, parent.Id, now);
        TodoListOperations.RemoveGroup(restored, temporary.Id, now);
        TodoListOperations.RestoreTask(restored, removed, now);
        Assert.True(parent.Subtasks.All(task => task.GroupId is null));
        Assert.Equal(2, restored.Tasks.Single().Subtasks.Count);
    }

    [Test]
    public static void LegacyAndDamagedSubtasksNormalizeWithoutLosingValidNestedSteps()
    {
        var root = new TodoTaskItem { Text = "Principal", IsCompleted = true, Subtasks =
            [new TodoTaskItem { Text = "Primer paso", Id = Guid.Empty, Subtasks = [new TodoTaskItem { Text = "Paso profundo" }] },
             new TodoTaskItem { Text = " " }] };
        var state = AppStateNormalizer.Normalize(new AppState { SchemaVersion = 3, TodoWindows = [new TodoWindowState { Tasks = [root] }] });
        Assert.Equal(AppConstants.CurrentSchemaVersion, state.SchemaVersion);
        Assert.Equal(2, root.Subtasks.Count);
        Assert.False(root.IsCompleted);
        Assert.True(root.Subtasks.All(child => child.Id != Guid.Empty && child.Subtasks.Count == 0));
        var legacy = AppStateNormalizer.Normalize(new AppState { SchemaVersion = 3, TodoWindows = [new TodoWindowState { Tasks = [new TodoTaskItem { Text = "Anterior" }] }] });
        Assert.Equal(0, legacy.TodoWindows[0].Tasks[0].Subtasks.Count);
    }
}
