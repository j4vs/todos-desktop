using Todos.Domain.Models;

namespace Todos.Domain;

public static class TodoListOperations
{
    public static TodoTaskItem AddTask(TodoWindowState window, string text, DateTimeOffset now, Guid? groupId = null)
    {
        ArgumentNullException.ThrowIfNull(window);
        ValidateGroup(window, groupId);
        var item = new TodoTaskItem
        {
            Text = TodoText.NormalizeTask(text),
            GroupId = groupId,
            PendingOrder = window.Tasks.Count(task => !task.IsCompleted && task.GroupId == groupId),
            CreatedAt = now,
            UpdatedAt = now,
        };

        window.Tasks.Add(item);
        Touch(window, now);
        return item;
    }

    public static void EditTask(TodoWindowState window, Guid taskId, string text, DateTimeOffset now)
    {
        var task = FindTask(window, taskId);
        task.Text = TodoText.NormalizeTask(text);
        task.UpdatedAt = now;
        Touch(window, now);
    }

    public static void CompleteTask(TodoWindowState window, Guid taskId, DateTimeOffset now)
    {
        var task = FindTask(window, taskId);
        if (task.IsCompleted)
        {
            return;
        }

        task.IsCompleted = true;
        task.CompletedAt = now;
        task.PendingOrder = -1;
        task.UpdatedAt = now;
        NormalizePendingOrder(window);
        Touch(window, now);
    }

    public static void ReopenTask(TodoWindowState window, Guid taskId, DateTimeOffset now)
    {
        var task = FindTask(window, taskId);
        if (!task.IsCompleted)
        {
            return;
        }

        task.IsCompleted = false;
        task.CompletedAt = null;
        task.PendingOrder = window.Tasks.Count(candidate => !candidate.IsCompleted && candidate.GroupId == task.GroupId);
        task.UpdatedAt = now;
        NormalizePendingOrder(window);
        Touch(window, now);
    }

    public static TodoTaskItem RemoveTask(TodoWindowState window, Guid taskId, DateTimeOffset now)
    {
        var task = FindTask(window, taskId);
        window.Tasks.Remove(task);
        NormalizePendingOrder(window);
        Touch(window, now);
        return task;
    }

    public static void RestoreTask(TodoWindowState window, TodoTaskItem task, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(task);

        if (window.Tasks.Any(candidate => candidate.Id == task.Id))
        {
            return;
        }

        window.Tasks.Add(task);
        if (task.GroupId is { } groupId && !window.Groups.Any(group => group.Id == groupId))
        {
            task.GroupId = null;
        }
        if (!task.IsCompleted)
        {
            NormalizePendingOrder(window);
        }

        task.UpdatedAt = now;
        Touch(window, now);
    }

    public static void MovePendingTask(TodoWindowState window, Guid taskId, int destinationIndex, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(window);
        var groupId = FindTask(window, taskId).GroupId;
        var pending = GetPending(window).Where(task => task.GroupId == groupId).ToList();
        var task = pending.SingleOrDefault(candidate => candidate.Id == taskId)
            ?? throw new KeyNotFoundException("No se encontró la tarea pendiente.");

        pending.Remove(task);
        destinationIndex = Math.Clamp(destinationIndex, 0, pending.Count);
        pending.Insert(destinationIndex, task);

        for (var index = 0; index < pending.Count; index++)
        {
            pending[index].PendingOrder = index;
        }

        task.UpdatedAt = now;
        Touch(window, now);
    }

    public static IReadOnlyList<TodoTaskItem> GetPending(TodoWindowState window) =>
        window.Tasks
            .Where(task => !task.IsCompleted)
            .OrderBy(task => task.PendingOrder)
            .ThenBy(task => task.CreatedAt)
            .ToArray();

    public static IReadOnlyList<TodoTaskItem> GetCompleted(TodoWindowState window) =>
        window.Tasks
            .Where(task => task.IsCompleted)
            .OrderByDescending(task => task.CompletedAt)
            .ThenByDescending(task => task.UpdatedAt)
            .ToArray();

    public static void NormalizePendingOrder(TodoWindowState window)
    {
        foreach (var group in GetPending(window).GroupBy(task => task.GroupId))
        {
            var index = 0;
            foreach (var task in group)
            {
                task.PendingOrder = index++;
            }
        }
    }

    public static TodoTaskGroup AddGroup(TodoWindowState window, string name, DateTimeOffset now)
    {
        var normalized = TodoText.NormalizeTitle(name);
        if (string.IsNullOrWhiteSpace(name) || normalized.Equals("General", StringComparison.OrdinalIgnoreCase) ||
            window.Groups.Any(group => group.Name.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Escribe un nombre de grupo distinto y no vacío.", nameof(name));
        }

        var group = new TodoTaskGroup { Name = normalized };
        window.Groups.Add(group);
        Touch(window, now);
        return group;
    }

    public static void RenameGroup(TodoWindowState window, Guid groupId, string name, DateTimeOffset now)
    {
        var group = window.Groups.Single(group => group.Id == groupId);
        var normalized = TodoText.NormalizeTitle(name);
        if (string.IsNullOrWhiteSpace(name) || normalized.Equals("General", StringComparison.OrdinalIgnoreCase) ||
            window.Groups.Any(candidate => candidate.Id != groupId && candidate.Name.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Escribe un nombre de grupo distinto y no vacío.", nameof(name));
        }

        group.Name = normalized;
        Touch(window, now);
    }

    public static void MoveTaskToGroup(TodoWindowState window, Guid taskId, Guid? groupId, int destinationIndex, DateTimeOffset now)
    {
        ValidateGroup(window, groupId);
        var task = FindTask(window, taskId);
        task.GroupId = groupId;
        if (!task.IsCompleted)
        {
            MovePendingTask(window, taskId, destinationIndex, now);
        }

        task.UpdatedAt = now;
        NormalizePendingOrder(window);
        Touch(window, now);
    }

    public static void RemoveGroup(TodoWindowState window, Guid groupId, DateTimeOffset now)
    {
        var group = window.Groups.Single(group => group.Id == groupId);
        foreach (var task in window.Tasks.Where(task => task.GroupId == groupId).OrderBy(task => task.PendingOrder).ToArray())
        {
            MoveTaskToGroup(window, task.Id, null, window.Tasks.Count, now);
        }

        window.Groups.Remove(group);
        Touch(window, now);
    }

    private static void ValidateGroup(TodoWindowState window, Guid? groupId)
    {
        if (groupId.HasValue && !window.Groups.Any(group => group.Id == groupId))
        {
            throw new ArgumentException("No se encontró el grupo.", nameof(groupId));
        }
    }

    private static TodoTaskItem FindTask(TodoWindowState window, Guid taskId)
    {
        ArgumentNullException.ThrowIfNull(window);
        return window.Tasks.SingleOrDefault(task => task.Id == taskId)
            ?? throw new KeyNotFoundException("No se encontró la tarea.");
    }

    private static void Touch(TodoWindowState window, DateTimeOffset now) => window.UpdatedAt = now;
}
