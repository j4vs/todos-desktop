using Todos.Domain;
using Todos.Domain.Models;

namespace Todos.Tests;

internal static class DomainTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 1, 12, 0, 0, TimeSpan.Zero);

    [Test]
    public static void TextNormalizationPreservesContentAndEnforcesLimits()
    {
        Assert.Equal("Línea uno Línea dos", TodoText.NormalizeTask("  Línea uno\r\n Línea dos  "));
        Assert.Equal(AppConstants.MaxTaskTextLength, TodoText.NormalizeTask(new string('a', 500)).Length);
        Assert.Equal(AppConstants.MaxTitleLength, TodoText.NormalizeTitle(new string('b', 120)).Length);
        Assert.Equal("Nueva lista", TodoText.NormalizeTitle("  "));
        Assert.Throws<ArgumentException>(() => TodoText.NormalizeTask(" \r\n "));
    }

    [Test]
    public static void TaskLifecycleMaintainsExpectedOrder()
    {
        var window = new TodoWindowState();
        var first = TodoListOperations.AddTask(window, "Backend", Now);
        var second = TodoListOperations.AddTask(window, "Frontend", Now.AddMinutes(1));
        var third = TodoListOperations.AddTask(window, "Pruebas", Now.AddMinutes(2));

        TodoListOperations.MovePendingTask(window, third.Id, 0, Now.AddMinutes(3));
        Assert.Equal(third.Id, TodoListOperations.GetPending(window)[0].Id);
        Assert.Equal(first.Id, TodoListOperations.GetPending(window)[1].Id);

        TodoListOperations.CompleteTask(window, first.Id, Now.AddMinutes(4));
        TodoListOperations.CompleteTask(window, second.Id, Now.AddMinutes(5));
        Assert.Equal(second.Id, TodoListOperations.GetCompleted(window)[0].Id);

        TodoListOperations.ReopenTask(window, first.Id, Now.AddMinutes(6));
        var pending = TodoListOperations.GetPending(window);
        Assert.Equal(first.Id, pending[^1].Id);
        Assert.SequenceEqual(Enumerable.Range(0, pending.Count), pending.Select(task => task.PendingOrder));
    }

    [Test]
    public static void EditDeleteAndRestoreKeepTaskIdentity()
    {
        var window = new TodoWindowState();
        var task = TodoListOperations.AddTask(window, "Texto original", Now);
        TodoListOperations.EditTask(window, task.Id, "Texto editado", Now.AddMinutes(1));
        Assert.Equal("Texto editado", task.Text);

        var removed = TodoListOperations.RemoveTask(window, task.Id, Now.AddMinutes(2));
        Assert.Equal(0, window.Tasks.Count);
        TodoListOperations.RestoreTask(window, removed, Now.AddMinutes(3));
        Assert.Equal(task.Id, window.Tasks.Single().Id);
    }

    [Test]
    public static void NormalizerRepairsInvalidAndDuplicateData()
    {
        var duplicatedWindowId = Guid.NewGuid();
        var duplicatedTaskId = Guid.NewGuid();
        var state = new AppState
        {
            SchemaVersion = 0,
            TodoWindows =
            [
                new TodoWindowState
                {
                    Id = duplicatedWindowId,
                    Title = " ",
                    Width = 10,
                    Height = double.NaN,
                    Tasks =
                    [
                        new TodoTaskItem { Id = duplicatedTaskId, Text = "Primera", PendingOrder = 50 },
                        new TodoTaskItem { Id = duplicatedTaskId, Text = "Segunda", PendingOrder = -8 },
                        new TodoTaskItem { Text = " " },
                    ],
                },
                new TodoWindowState { Id = duplicatedWindowId },
            ],
        };

        AppStateNormalizer.Normalize(state);

        Assert.Equal(AppConstants.CurrentSchemaVersion, state.SchemaVersion);
        Assert.Equal("Nueva lista", state.TodoWindows[0].Title);
        Assert.Equal(AppConstants.MinimumWindowWidth, state.TodoWindows[0].Width);
        Assert.Equal(AppConstants.DefaultWindowHeight, state.TodoWindows[0].Height);
        Assert.Equal(2, state.TodoWindows[0].Tasks.Count);
        Assert.False(state.TodoWindows[0].Tasks[0].Id == state.TodoWindows[0].Tasks[1].Id);
        Assert.False(state.TodoWindows[0].Id == state.TodoWindows[1].Id);
        Assert.SequenceEqual(new[] { 0, 1 }, TodoListOperations.GetPending(state.TodoWindows[0]).Select(task => task.PendingOrder));
    }

    [Test]
    public static void FutureSchemaIsRejectedWithoutMutation()
    {
        var state = new AppState { SchemaVersion = AppConstants.CurrentSchemaVersion + 1 };
        var exception = Assert.Throws<UnsupportedSchemaVersionException>(() => AppStateNormalizer.Normalize(state));
        Assert.Equal(AppConstants.CurrentSchemaVersion + 1, exception.SchemaVersion);
    }
}
