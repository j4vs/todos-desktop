using Todos.Domain.Models;

namespace Todos.Domain;

public static class RecycleBinOperations
{
    public static bool MoveToTrash(AppState state, Guid id, DateTimeOffset now)
    {
        var window = state.TodoWindows.FirstOrDefault(item => item.Id == id);
        if (window is null) return false;
        state.TodoWindows.Remove(window);
        window.IsVisible = false;
        window.TrashedAt = now;
        state.TrashedWindows.Add(window);
        if (state.LauncherState.LastMinimizedWindowId == id)
            state.LauncherState.LastMinimizedWindowId = null;
        return true;
    }

    public static TodoWindowState? Restore(AppState state, Guid id)
    {
        var window = state.TrashedWindows.FirstOrDefault(item => item.Id == id);
        if (window is null) return null;
        state.TrashedWindows.Remove(window);
        window.TrashedAt = null;
        window.IsVisible = true;
        state.TodoWindows.Add(window);
        return window;
    }

    public static bool DeletePermanently(AppState state, Guid id) =>
        state.TrashedWindows.RemoveAll(window => window.Id == id) > 0;
}
