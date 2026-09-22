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
        state.TodoWindows ??= [];

        var knownWindowIds = new HashSet<Guid>();
        foreach (var window in state.TodoWindows)
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
                    window.Tasks.RemoveAt(index);
                    continue;
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

            TodoListOperations.NormalizePendingOrder(window);
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
