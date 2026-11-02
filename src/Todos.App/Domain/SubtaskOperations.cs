using Todos.Domain.Models;

namespace Todos.Domain;

public static class SubtaskOperations
{
    public static TodoTaskItem Find(TodoWindowState window, Guid id) =>
        window.Tasks.SelectMany(root => new[] { root }.Concat(root.Subtasks)).SingleOrDefault(task => task.Id == id)
        ?? throw new KeyNotFoundException("No se encontró la tarea.");

    public static TodoTaskItem? Parent(TodoWindowState window, Guid id) =>
        window.Tasks.FirstOrDefault(root => root.Subtasks.Any(child => child.Id == id));

    public static TodoTaskItem Add(TodoWindowState window, Guid parentId, string text, DateTimeOffset now)
    {
        var parent = window.Tasks.Single(root => root.Id == parentId);
        var child = new TodoTaskItem { Text = TodoText.NormalizeTask(text), GroupId = parent.GroupId, CreatedAt = now, UpdatedAt = now };
        parent.Subtasks.Add(child);
        parent.UpdatedAt = now;
        parent.AreSubtasksExpanded = true;
        SynchronizeParent(window, parent, now);
        window.UpdatedAt = now;
        return child;
    }

    public static void SetCompleted(TodoWindowState window, Guid id, bool completed, DateTimeOffset now)
    {
        var task = Find(window, id);
        if (task.IsCompleted == completed) return;
        var parent = Parent(window, id);
        if (parent is null)
            foreach (var child in task.Subtasks) SetStatus(child, completed, now);
        SetStatus(task, completed, now);
        if (!completed && parent is null) task.PendingOrder = window.Tasks.Count(candidate => candidate.Id != id && !candidate.IsCompleted && candidate.GroupId == task.GroupId);
        if (parent is not null) SynchronizeParent(window, parent, now);
        TodoListOperations.NormalizePendingOrder(window);
        window.UpdatedAt = now;
    }

    public static void SynchronizeParent(TodoWindowState window, TodoTaskItem parent, DateTimeOffset now)
    {
        if (parent.Subtasks.Count > 0)
        {
            var completed = parent.Subtasks.All(child => child.IsCompleted);
            var wasCompleted = parent.IsCompleted;
            SetStatus(parent, completed, now);
            if (wasCompleted && !completed)
                parent.PendingOrder = window.Tasks.Count(candidate => candidate.Id != parent.Id && !candidate.IsCompleted && candidate.GroupId == parent.GroupId);
        }
        TodoListOperations.NormalizePendingOrder(window);
    }

    public static void Convert(TodoWindowState window, Guid sourceId, Guid destinationId, DateTimeOffset now)
    {
        var source = window.Tasks.Single(root => root.Id == sourceId);
        var destination = window.Tasks.Single(root => root.Id == destinationId);
        if (source == destination || source.GroupId != destination.GroupId || source.Subtasks.Count > 0)
            throw new ArgumentException("Elige otra tarea del mismo grupo. Solo se convierten tareas sin subtareas.");
        window.Tasks.Remove(source);
        destination.Subtasks.Add(source);
        destination.UpdatedAt = now;
        destination.AreSubtasksExpanded = true;
        source.UpdatedAt = now;
        SynchronizeParent(window, destination, now);
        window.UpdatedAt = now;
    }

    public static void Move(TodoWindowState window, Guid childId, int destinationIndex, DateTimeOffset now)
    {
        var parent = Parent(window, childId) ?? throw new ArgumentException("No se encontró la tarea principal.");
        var child = parent.Subtasks.Single(task => task.Id == childId);
        parent.Subtasks.Remove(child);
        parent.Subtasks.Insert(Math.Clamp(destinationIndex, 0, parent.Subtasks.Count), child);
        child.UpdatedAt = parent.UpdatedAt = window.UpdatedAt = now;
    }

    private static void SetStatus(TodoTaskItem task, bool completed, DateTimeOffset now)
    {
        if (task.IsCompleted == completed) return;
        task.IsCompleted = completed;
        task.CompletedAt = completed ? now : null;
        if (completed) task.PendingOrder = -1;
        task.UpdatedAt = now;
    }
}
