using Todos.Application.Windows;
using Todos.Domain.Models;
using Todos.Presentation.Views;

namespace Todos.Presentation.Windows;

public sealed class TodoWindowManager
{
    private readonly Func<TodoWindowState, TodoWindow> _windowFactory;
    private readonly IDisplayService _displayService;
    private readonly Action _keepLauncherAbove;
    private readonly Dictionary<Guid, TodoWindow> _windows = [];

    public TodoWindowManager(
        Func<TodoWindowState, TodoWindow> windowFactory,
        IDisplayService displayService,
        Action keepLauncherAbove)
    {
        _windowFactory = windowFactory;
        _displayService = displayService;
        _keepLauncherAbove = keepLauncherAbove;
    }

    public IReadOnlyCollection<Guid> CreatedWindowIds => _windows.Keys;

    public TodoWindow? GetWindow(Guid id) => _windows.GetValueOrDefault(id);

    public void Show(TodoWindowState state)
    {
        if (!_windows.TryGetValue(state.Id, out var window))
        {
            window = _windowFactory(state);
            window.Activated += (_, _) => _keepLauncherAbove();
            _windows[state.Id] = window;
        }

        var area = _displayService.GetBestWorkingArea(state.PositionX, state.PositionY);
        var position = WindowPositioning.EnsureVisible(
            state.PositionX,
            state.PositionY,
            state.Width,
            state.Height,
            area);
        state.PositionX = position.X;
        state.PositionY = position.Y;
        window.Left = position.X;
        window.Top = position.Y;
        window.Width = state.Width;
        window.Height = state.Height;
        state.IsVisible = true;

        if (!window.IsVisible)
        {
            window.Show();
        }

        window.Topmost = false;
        window.Topmost = true;
        window.Activate();
        _keepLauncherAbove();
    }

    public void Hide(TodoWindowState state)
    {
        state.IsVisible = false;
        if (_windows.TryGetValue(state.Id, out var window))
        {
            window.Hide();
        }
    }

    public void Remove(TodoWindowState state)
    {
        if (_windows.Remove(state.Id, out var window))
        {
            window.AllowClose = true;
            window.Close();
        }
    }

    public void ShowAll(IEnumerable<TodoWindowState> states)
    {
        foreach (var state in states)
        {
            Show(state);
        }
    }

    public void HideAll(IEnumerable<TodoWindowState> states)
    {
        foreach (var state in states)
        {
            Hide(state);
        }
    }

    public void EnsureVisible(IEnumerable<TodoWindowState> states)
    {
        foreach (var state in states.Where(state => state.IsVisible))
        {
            var area = _displayService.GetBestWorkingArea(state.PositionX, state.PositionY);
            var position = WindowPositioning.EnsureVisible(
                state.PositionX,
                state.PositionY,
                state.Width,
                state.Height,
                area);
            state.PositionX = position.X;
            state.PositionY = position.Y;

            if (_windows.TryGetValue(state.Id, out var window))
            {
                window.Left = position.X;
                window.Top = position.Y;
            }
        }
    }

    public void CloseAll()
    {
        foreach (var window in _windows.Values)
        {
            window.AllowClose = true;
            window.Close();
        }

        _windows.Clear();
    }
}
