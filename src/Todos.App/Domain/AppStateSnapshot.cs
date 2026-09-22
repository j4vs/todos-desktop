using Todos.Domain.Models;

namespace Todos.Domain;

public static class AppStateSnapshot
{
    public static AppState Clone(AppState source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new AppState
        {
            SchemaVersion = source.SchemaVersion,
            LastSavedAt = source.LastSavedAt,
            Settings = new AppSettings
            {
                RunAtWindowsStartup = source.Settings.RunAtWindowsStartup,
                HasSeenFirstRunHint = source.Settings.HasSeenFirstRunHint,
            },
            LauncherState = new LauncherState
            {
                PositionX = source.LauncherState.PositionX,
                PositionY = source.LauncherState.PositionY,
                DisplayId = source.LauncherState.DisplayId,
            },
            TodoWindows = source.TodoWindows.Select(CloneWindow).ToList(),
        };
    }

    private static TodoWindowState CloneWindow(TodoWindowState source) => new()
    {
        Id = source.Id,
        Title = source.Title,
        ColorTheme = source.ColorTheme,
        PositionX = source.PositionX,
        PositionY = source.PositionY,
        Width = source.Width,
        Height = source.Height,
        IsVisible = source.IsVisible,
        AreCompletedTasksExpanded = source.AreCompletedTasksExpanded,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt,
        Tasks = source.Tasks.Select(CloneTask).ToList(),
        Groups = source.Groups.Select(group => new TodoTaskGroup { Id = group.Id, Name = group.Name, IsExpanded = group.IsExpanded }).ToList(),
        IsGeneralGroupExpanded = source.IsGeneralGroupExpanded,
    };

    private static TodoTaskItem CloneTask(TodoTaskItem source) => new()
    {
        Id = source.Id,
        Text = source.Text,
        IsCompleted = source.IsCompleted,
        PendingOrder = source.PendingOrder,
        GroupId = source.GroupId,
        CompletedAt = source.CompletedAt,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt,
    };
}
