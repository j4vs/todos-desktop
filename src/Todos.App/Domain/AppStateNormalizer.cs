using Todos.Domain.Models;

namespace Todos.Domain;

public static class AppStateNormalizer
{
    public static AppState Normalize(AppState? state)
    {
        state ??= new AppState();
        if (state.SchemaVersion > AppConstants.CurrentSchemaVersion)
        {
            throw new UnsupportedSchemaVersionException(state.SchemaVersion);
        }

        state.SchemaVersion = AppConstants.CurrentSchemaVersion;
        state.LauncherState ??= new LauncherState();
        state.Settings ??= new AppSettings();
        state.Settings.FontSizeStep = Math.Clamp(state.Settings.FontSizeStep, -2, 4);
        if (state.Settings.LauncherSize is < 0 or > 2) state.Settings.LauncherSize = 1;
        if (state.Settings.SpacingSize is < 0 or > 2) state.Settings.SpacingSize = 1;
        state.TodoWindows ??= [];
        state.TrashedWindows ??= [];
        foreach (var trashed in state.TrashedWindows)
        {
            trashed.IsVisible = false;
            trashed.TrashedAt ??= trashed.UpdatedAt == default ? DateTimeOffset.UtcNow : trashed.UpdatedAt;
        }
        foreach (var active in state.TodoWindows) active.TrashedAt = null;

        var knownWindowIds = new HashSet<Guid>();
        foreach (var window in state.TodoWindows.Concat(state.TrashedWindows))
        {
            if (window.Id == Guid.Empty || !knownWindowIds.Add(window.Id))
            {
                window.Id = Guid.NewGuid();
                knownWindowIds.Add(window.Id);
            }

            window.Title = TodoText.NormalizeTitle(window.Title);
            window.Width = NormalizeWindowWidth(window.Width);
            window.Height = NormalizeDimension(window.Height, AppConstants.MinimumWindowHeight, AppConstants.DefaultWindowHeight);
            window.Tasks ??= [];
            window.Groups ??= [];
            var knownGroupIds = new HashSet<Guid>();
            foreach (var group in window.Groups)
            {
                if (group.Id == Guid.Empty || !knownGroupIds.Add(group.Id))
                {
                    group.Id = Guid.NewGuid();
                    knownGroupIds.Add(group.Id);
                }

                group.Name = TodoText.NormalizeTitle(group.Name);
            }

            var knownTaskIds = new HashSet<Guid>();
            for (var index = window.Tasks.Count - 1; index >= 0; index--)
            {
                var task = window.Tasks[index];
                task.NotesMarkdown ??= string.Empty;
                if (task.GroupId is { } groupId && !knownGroupIds.Contains(groupId))
                {
                    task.GroupId = null;
                }
                try
                {
                    task.Text = TodoText.NormalizeTask(task.Text);
                }
                catch (ArgumentException)
                {
                    if (task.Subtasks?.Count > 0 || !string.IsNullOrWhiteSpace(task.NotesMarkdown)) task.Text = "Tarea";
                    else { window.Tasks.RemoveAt(index); continue; }
                }

                if (task.Id == Guid.Empty || !knownTaskIds.Add(task.Id))
                {
                    task.Id = Guid.NewGuid();
                    knownTaskIds.Add(task.Id);
                }

                if (task.IsCompleted)
                {
                    task.CompletedAt ??= task.UpdatedAt == default ? task.CreatedAt : task.UpdatedAt;
                    task.PendingOrder = -1;
                }
                else
                {
                    task.CompletedAt = null;
                }
            }

            foreach (var parent in window.Tasks)
            {
                parent.Subtasks ??= [];
                // Repair deeper imported trees by keeping every valid step at one level.
                var queue = new Queue<TodoTaskItem>(parent.Subtasks);
                var repaired = new List<TodoTaskItem>();
                while (queue.TryDequeue(out var child))
                {
                    child.NotesMarkdown ??= string.Empty;
                    if (child.Subtasks is not null) foreach (var nested in child.Subtasks) queue.Enqueue(nested);
                    child.Subtasks = [];
                    try { child.Text = TodoText.NormalizeTask(child.Text); }
                    catch (ArgumentException)
                    {
                        if (string.IsNullOrWhiteSpace(child.NotesMarkdown)) continue;
                        child.Text = "Subtarea";
                    }
                    if (child.Id == Guid.Empty || !knownTaskIds.Add(child.Id)) { child.Id = Guid.NewGuid(); knownTaskIds.Add(child.Id); }
                    child.GroupId = parent.GroupId;
                    if (child.IsCompleted) child.CompletedAt ??= child.UpdatedAt == default ? child.CreatedAt : child.UpdatedAt;
                    else child.CompletedAt = null;
                    repaired.Add(child);
                }
                parent.Subtasks = repaired;
                SubtaskOperations.SynchronizeParent(window, parent, parent.UpdatedAt == default ? DateTimeOffset.UtcNow : parent.UpdatedAt);
            }
            TodoListOperations.NormalizePendingOrder(window);
        }

        if (!state.TodoWindows.Any(window => window.Id == state.LauncherState.LastMinimizedWindowId))
        {
            state.LauncherState.LastMinimizedWindowId = null;
        }

        return state;
    }

    private static double NormalizeDimension(double value, double minimum, double defaultValue) =>
        double.IsFinite(value) ? Math.Max(value, minimum) : defaultValue;

    private static double NormalizeWindowWidth(double value)
    {
        if (Math.Abs(value - 340) < 0.01 || Math.Abs(value - 380) < 0.01)
        {
            return AppConstants.DefaultWindowWidth;
        }

        return NormalizeDimension(value, AppConstants.MinimumWindowWidth, AppConstants.DefaultWindowWidth);
    }
}
