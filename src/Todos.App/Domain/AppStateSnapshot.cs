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
                FontSizeStep = source.Settings.FontSizeStep,
                LauncherSize = source.Settings.LauncherSize,
                SpacingSize = source.Settings.SpacingSize,
                RunAtWindowsStartup = source.Settings.RunAtWindowsStartup,
                AutoMinimizeWindows = source.Settings.AutoMinimizeWindows,
                HasSeenFirstRunHint = source.Settings.HasSeenFirstRunHint,
            },
            LauncherState = new LauncherState
            {
                PositionX = source.LauncherState.PositionX,
                PositionY = source.LauncherState.PositionY,
                DisplayId = source.LauncherState.DisplayId,
                LastMinimizedWindowId = source.LauncherState.LastMinimizedWindowId,
            },
            TodoWindows = source.TodoWindows.Select(CloneWindow).ToList(),
            TrashedWindows = source.TrashedWindows.Select(CloneWindow).ToList(),
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
        TrashedAt = source.TrashedAt,
        UpdatedAt = source.UpdatedAt,
        Tasks = source.Tasks.Select(CloneTask).ToList(),
        Groups = source.Groups.Select(group => new TodoTaskGroup { Id = group.Id, Name = group.Name, IsExpanded = group.IsExpanded }).ToList(),
        IsGeneralGroupExpanded = source.IsGeneralGroupExpanded,
    };

    public static TodoTaskItem CloneTask(TodoTaskItem source) => new()
    {
        NotesMarkdown = source.NotesMarkdown,
        Subtasks = source.Subtasks.Select(CloneTask).ToList(),
        AreSubtasksExpanded = source.AreSubtasksExpanded,
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
