using Todos.Application;
using Todos.Application.Persistence;
using Todos.Application.Windows;
using Todos.Domain;
using Todos.Domain.Models;
using Todos.Infrastructure.Logging;
using Todos.Infrastructure.Windows;
using Todos.Presentation.ViewModels;
using Todos.Presentation.Views;
using Todos.Presentation.Windows;

namespace Todos.Presentation;

public sealed class AppController : IDisposable
{
    private readonly AppState _state;
    private readonly DebouncedAppStateSaver _saver;
    private readonly string? _loadWarning;
    private readonly AppLaunchOptions _launchOptions;
    private readonly IStartupRegistrationService _startupRegistration;
    private readonly LocalLogService _log;
    private readonly IDisplayService _displayService = new WindowsDisplayService();
    private readonly TodoWindowManager _windowManager;
    private LauncherViewModel? _launcherViewModel;
    private LauncherWindow? _launcherWindow;
    private NotificationAreaService? _notificationArea;
    private bool _isExiting;
    private bool _disposed;
    private OutsideClickMonitor? _outsideClicks;
    private RecycleBinWindow? _recycleBinWindow;
    private RecycleBinViewModel? _recycleBinViewModel;

    public AppController(
        AppState state,
        DebouncedAppStateSaver saver,
        string? loadWarning,
        AppLaunchOptions launchOptions,
        IStartupRegistrationService startupRegistration,
        LocalLogService log)
    {
        _state = state;
        _saver = saver;
        _saver.SaveCompleted += OnSaveCompleted;
        _loadWarning = loadWarning;
        _launchOptions = launchOptions;
        _startupRegistration = startupRegistration;
        _log = log;
        _windowManager = new TodoWindowManager(
            CreateTodoWindow,
            _displayService,
            () => _launcherWindow?.KeepAboveTodoWindows());
    }

    public async Task StartAsync()
    {
        Todos.Presentation.Themes.AppearanceResources.Apply(_state.Settings);
        SynchronizeStartupRegistration();
        SubscribeToSystemEvents();

        _launcherViewModel = new LauncherViewModel(
            _state.Settings.RunAtWindowsStartup,
            CreateList,
            ShowAll,
            HideAll,
            ShowHidden,
            DeleteHidden,
            ChangeStartupPreference,
            ExitAsync,
            _state.Settings.AutoMinimizeWindows,
            enabled => { _state.Settings.AutoMinimizeWindows = enabled; ScheduleSave(); },
            id => { if (_state.TodoWindows.FirstOrDefault(window => window.Id == id) is { } state) HideWindow(state); },
            ShowRecycleBin,
            HideLauncher,
            _state.Settings, ApplyAppearance)
        {
            StatusMessage = _loadWarning ?? _loadStartupWarning,
        };

        _launcherWindow = new LauncherWindow(_launcherViewModel);
        _launcherWindow.PositionCommitted += OnLauncherPositionCommitted;
        var area = _displayService.GetBestWorkingArea(
            _state.LauncherState.PositionX,
            _state.LauncherState.PositionY);
        var position = WindowPositioning.EnsureVisible(
            _state.LauncherState.PositionX,
            _state.LauncherState.PositionY,
            _launcherWindow.Width,
            _launcherWindow.Height,
            area);
        _launcherWindow.Left = position.X;
        _launcherWindow.Top = position.Y;

        System.Windows.Application.Current.MainWindow = _launcherWindow;
        _launcherWindow.Show();
        _outsideClicks = new OutsideClickMonitor((point, primaryButton) =>
        {
            if (primaryButton && _launcherWindow.ContainsPrimaryButton(point)) _launcherWindow.CapturePrimaryClick();
            return OnOutsideClick(point);
        });
        _notificationArea = new NotificationAreaService(ShowLauncher, CreateList, RequestExit, ToggleLauncherVisibility,
            () => _launcherWindow?.IsVisible == true);
        _notificationArea.UpdateAppearance(1 + _state.Settings.FontSizeStep * .1);
        _launcherWindow.IsVisibleChanged += (_, _) => _notificationArea?.UpdateLauncherVisibility(_launcherWindow.IsVisible);

        foreach (var todoState in _state.TodoWindows.Where(window => window.IsVisible))
        {
            _windowManager.Show(todoState);
        }

        RefreshHiddenWindows();

        if (_launchOptions.RenderRecycleBinPreviewPath is not null)
        {
            ShowRecycleBin();
            await Task.Delay(350);
            await WindowPreviewRenderer.RenderPngAsync(_recycleBinWindow!, _launchOptions.RenderRecycleBinPreviewPath);
            await ExitAsync();
            return;
        }

        if (_launchOptions.RenderLauncherPreviewPath is not null)
        {
            _launcherViewModel.IsMenuOpen = true;
            await Task.Delay(350);
            await WindowPreviewRenderer.RenderPngAsync(
                _launcherWindow.MenuPreviewContent,
                _launchOptions.RenderLauncherPreviewPath);
            var previewDirectory = Path.GetDirectoryName(_launchOptions.RenderLauncherPreviewPath) ?? string.Empty;
            var previewName = Path.GetFileNameWithoutExtension(_launchOptions.RenderLauncherPreviewPath);
            var previewExtension = Path.GetExtension(_launchOptions.RenderLauncherPreviewPath);
            var hiddenPreviewPath = Path.Combine(previewDirectory, $"{previewName}-hidden{previewExtension}");
            _launcherViewModel.IsMenuOpen = false;
            _launcherViewModel.ShowHiddenLists();
            await Task.Delay(350);
            await WindowPreviewRenderer.RenderPngAsync(
                _launcherWindow.HiddenSubmenuPreviewContent,
                hiddenPreviewPath);
            var buttonPreviewPath = Path.Combine(previewDirectory, $"{previewName}-button{previewExtension}");
            await WindowPreviewRenderer.RenderPngAsync(
                _launcherWindow.LauncherButtonPreviewContent,
                buttonPreviewPath);
            await ExitAsync();
            return;
        }

        if (_launchOptions.RenderPreviewPath is not null)
        {
            if (_state.TodoWindows.Count == 0)
            {
                CreateList();
            }

            var previewState = _state.TodoWindows.First();
            _windowManager.Show(previewState);
            await Task.Delay(350);
            var previewWindow = _windowManager.GetWindow(previewState.Id)
                ?? throw new InvalidOperationException("No se pudo crear la ventana para la vista previa.");
            if (_launchOptions.RenderPreviewEditing &&
                previewWindow.DataContext is TodoWindowViewModel previewViewModel &&
                previewViewModel.PendingTasks.FirstOrDefault() is { } previewTask)
            {
                previewTask.BeginEditCommand.Execute(null);
                await Task.Delay(120);
            }
            await WindowPreviewRenderer.RenderPngAsync(previewWindow, _launchOptions.RenderPreviewPath);
            await ExitAsync();
            return;
        }

        if (_launchOptions.IsSmokeTest)
        {
            if (_state.TodoWindows.Count == 0)
            {
                CreateList();
            }

            // An isolated smoke run must verify persistence even when no new list is needed.
            if (_launchOptions.DataRoot is not null) ScheduleSave();
            await Task.Delay(_launchOptions.SmokeDelayMilliseconds);
            await ExitAsync();
        }
    }

    public void ShowFromExternalActivation()
    {
        ShowLauncher();
        if (_launcherViewModel is not null)
        {
            _launcherViewModel.IsMenuOpen = true;
            _launcherViewModel.StatusMessage = "Todo's ya estaba abierto.";
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _saver.SaveCompleted -= OnSaveCompleted;
        _outsideClicks?.Dispose();
        UnsubscribeFromSystemEvents();
        _notificationArea?.Dispose();
        _recycleBinWindow?.Close();
        _windowManager.CloseAll();
        if (!_isExiting)
        {
            _saver.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }

    private void CreateList()
    {
        var number = _state.TodoWindows.Count + 1;
        var state = new TodoWindowState
        {
            Title = number == 1 ? "Nueva lista" : $"Nueva lista {number}",
        };

        var area = _displayService.GetBestWorkingArea(_launcherWindow?.Left, _launcherWindow?.Top);
        var launcherLeft = _launcherWindow?.Left ?? area.Right - AppConstants.LauncherSize - 24;
        var launcherTop = _launcherWindow?.Top ?? area.Top + 24;
        var cascadeOffset = (_state.TodoWindows.Count % 6) * 22;
        var preferredX = launcherLeft - state.Width - 18 + cascadeOffset;
        var preferredY = launcherTop - 20 + cascadeOffset;
        var position = WindowPositioning.EnsureVisible(
            preferredX,
            preferredY,
            state.Width,
            state.Height,
            area);
        state.PositionX = position.X;
        state.PositionY = position.Y;

        _state.TodoWindows.Add(state);
        _windowManager.Show(state);
        RefreshHiddenWindows();
        ScheduleSave();
        CloseLauncherMenu();
    }

    private TodoWindow CreateTodoWindow(TodoWindowState state)
    {
        var viewModel = new TodoWindowViewModel(
            state,
            ScheduleSave,
            () => HideWindow(state),
            () => RequestDelete(state));
        return new TodoWindow(viewModel);
    }

    private void ShowAll()
    {
        _windowManager.ShowAll(_state.TodoWindows);
        RefreshHiddenWindows();
        ScheduleSave();
        CloseLauncherMenu();
        ShowLauncher();
    }

    private void HideAll()
    {
        _windowManager.HideAll(_state.TodoWindows);
        RefreshHiddenWindows();
        ScheduleSave();
        CloseLauncherMenu();
    }

    private void ShowHidden(Guid id)
    {
        var state = _state.TodoWindows.SingleOrDefault(window => window.Id == id);
        if (state is null)
        {
            return;
        }

        _windowManager.Show(state);
        RefreshHiddenWindows();
        ScheduleSave();
        CloseLauncherMenu();
    }

    private void HideWindow(TodoWindowState state)
    {
        if (!state.IsVisible || _isExiting) return;
        _state.LauncherState.LastMinimizedWindowId = state.Id;
        _windowManager.Hide(state);
        RefreshHiddenWindows();
        ScheduleSave();
    }

    private void DeleteHidden(Guid id)
    {
        var state = _state.TodoWindows.SingleOrDefault(window => window.Id == id);
        if (state is not null)
        {
            RequestDelete(state);
        }
    }

    private void RequestDelete(TodoWindowState state)
    {
        if (!RecycleBinOperations.MoveToTrash(_state, state.Id, DateTimeOffset.UtcNow)) return;
        _windowManager.Remove(state);
        _recycleBinViewModel?.Refresh(_state.TrashedWindows);
        RefreshHiddenWindows();
        ScheduleSave();
    }

    private void ShowRecycleBin()
    {
        CloseLauncherMenu();
        if (_recycleBinWindow is null)
        {
            _recycleBinViewModel = new RecycleBinViewModel(RestoreFromTrash, DeletePermanently);
            _recycleBinWindow = new RecycleBinWindow(_recycleBinViewModel);
            _recycleBinWindow.Closed += (_, _) => { _recycleBinWindow = null; _recycleBinViewModel = null; };
            var area = _displayService.GetBestWorkingArea(_launcherWindow?.Left, _launcherWindow?.Top);
            _recycleBinWindow.Width = Math.Min(460, area.Width - 16);
            _recycleBinWindow.Height = Math.Min(520, area.Height - 16);
            var position = WindowPositioning.EnsureVisible(_launcherWindow?.Left, _launcherWindow?.Top,
                _recycleBinWindow.Width, _recycleBinWindow.Height, area);
            _recycleBinWindow.Left = position.X;
            _recycleBinWindow.Top = position.Y;
        }
        _recycleBinViewModel!.Refresh(_state.TrashedWindows);
        _recycleBinWindow.Show();
        _recycleBinWindow.Activate();
    }

    private void RestoreFromTrash(Guid id)
    {
        if (RecycleBinOperations.Restore(_state, id) is not { } state) return;
        _windowManager.Show(state);
        _recycleBinViewModel?.Refresh(_state.TrashedWindows);
        RefreshHiddenWindows();
        ScheduleSave();
    }

    private void DeletePermanently(Guid id)
    {
        if (!RecycleBinOperations.DeletePermanently(_state, id)) return;
        _recycleBinViewModel?.Refresh(_state.TrashedWindows);
        ScheduleSave();
    }

    private void ApplyAppearance()
    {
        Todos.Presentation.Themes.AppearanceResources.Apply(_state.Settings);
        _notificationArea?.UpdateAppearance(1 + _state.Settings.FontSizeStep * .1);
        if (_launcherWindow is not null) OnLauncherPositionCommitted(_launcherWindow.Left, _launcherWindow.Top);
        ScheduleSave();
    }

    private void HideLauncher() =>
        System.Windows.Application.Current.Dispatcher.Invoke(() => _launcherWindow?.MinimizeLauncher());

    private void ToggleLauncherVisibility()
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            if (_launcherWindow?.IsVisible == true) HideLauncher();
            else ShowLauncher();
        });
    }

    private void ShowLauncher()
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() => _launcherWindow?.BringLauncherToFront());
    }

    private void RequestExit() =>
        _ = System.Windows.Application.Current.Dispatcher.InvokeAsync(async () => await ExitAsync());

    private void OnLauncherPositionCommitted(double x, double y)
    {
        var area = _displayService.GetBestWorkingArea(x, y);
        var position = WindowPositioning.EnsureVisible(
            x,
            y,
            _launcherWindow?.Width ?? AppConstants.LauncherWidth,
            _launcherWindow?.Height ?? AppConstants.LauncherHeight,
            area);

        if (_launcherWindow is not null)
        {
            _launcherWindow.Left = position.X;
            _launcherWindow.Top = position.Y;
        }

        _state.LauncherState.PositionX = position.X;
        _state.LauncherState.PositionY = position.Y;
        ScheduleSave();
    }

    private void RefreshHiddenWindows()
    {
        _launcherViewModel?.RefreshHiddenWindows(_state.TodoWindows);
        _launcherViewModel?.RefreshAssociation(_state.TodoWindows, _state.LauncherState.LastMinimizedWindowId);
    }

    private Action OnOutsideClick(System.Windows.Point screenPoint)
    {
        // Capture before the clicked button can show a formerly hidden window.
        var closeLauncher = _launcherWindow is not null && !_launcherWindow.ContainsInteraction(screenPoint);
        var candidates = _state.Settings.AutoMinimizeWindows && !_isExiting
            ? _state.TodoWindows.Where(state => state.IsVisible &&
                _windowManager.GetWindow(state.Id) is { } window && !window.ContainsInteraction(screenPoint)).ToArray()
            : [];
        return () =>
        {
            if (_isExiting) return;
            if (closeLauncher) _launcherWindow?.DismissPanels();
            if (!_state.Settings.AutoMinimizeWindows) return;
            foreach (var state in candidates)
            {
                if (_state.TodoWindows.Contains(state)) HideWindow(state);
            }
        };
    }

    private void ScheduleSave()
    {
        _launcherViewModel?.RefreshAssociation(_state.TodoWindows, _state.LauncherState.LastMinimizedWindowId);
        _saver.Schedule(_state);
    }

    private void OnSaveCompleted(AppState snapshot, Exception? error)
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.HasShutdownStarted) return;
        dispatcher.BeginInvoke(new Action(() =>
        {
            if (_disposed) return;
            foreach (var id in _windowManager.CreatedWindowIds)
            {
                if (_windowManager.GetWindow(id)?.DataContext is not TodoWindowViewModel vm) continue;
                var savedTasks = snapshot.TodoWindows.FirstOrDefault(window => window.Id == id)?.Tasks
                    .SelectMany(root => new[] { root }.Concat(root.Subtasks)).ToDictionary(task => task.Id);
                if (savedTasks is null) continue;
                foreach (var task in vm.PendingTasks.Concat(vm.CompletedTasks).SelectMany(root => new[] { root }.Concat(root.Subtasks)))
                    if (savedTasks.TryGetValue(task.Id, out var saved) && saved.NotesMarkdown == task.NotesMarkdown)
                        task.NotesSaveStatus = error is null ? "Guardado" : "No se pudo guardar. Conserva una copia con Exportar.";
            }
        }));
    }

    private bool ChangeStartupPreference(bool enable)
    {
        try
        {
            if (enable)
            {
                _startupRegistration.Enable();
            }
            else
            {
                _startupRegistration.Disable();
            }

            _state.Settings.RunAtWindowsStartup = enable;
            ScheduleSave();
            _log.Information(enable
                ? "Windows startup registration enabled."
                : "Windows startup registration disabled.");
            return true;
        }
        catch (Exception exception)
        {
            _log.Error("Windows startup registration could not be changed.", exception);
            if (_launcherViewModel is not null)
            {
                _launcherViewModel.StatusMessage =
                    "Windows no permitió cambiar el inicio automático.";
            }

            return false;
        }
    }

    private void SynchronizeStartupRegistration()
    {
        if (_launchOptions.IsSmokeTest ||
            _launchOptions.RenderPreviewPath is not null ||
            _launchOptions.RenderLauncherPreviewPath is not null ||
            _launchOptions.RenderRecycleBinPreviewPath is not null)
        {
            return;
        }

        try
        {
            var status = _startupRegistration.GetStatus();
            if (_state.Settings.RunAtWindowsStartup)
            {
                if (!status.IsRegistered || !status.MatchesCurrentExecutable)
                {
                    _startupRegistration.Enable();
                }
            }
            else if (status.IsRegistered)
            {
                _startupRegistration.Disable();
            }
        }
        catch (Exception exception)
        {
            _log.Error("Windows startup registration could not be synchronized.", exception);
            _loadStartupWarning = "No se pudo comprobar el inicio automático con Windows.";
        }
    }

    private string? _loadStartupWarning;

    private void SubscribeToSystemEvents()
    {
        Microsoft.Win32.SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        Microsoft.Win32.SystemEvents.PowerModeChanged += OnPowerModeChanged;
    }

    private void UnsubscribeFromSystemEvents()
    {
        Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        Microsoft.Win32.SystemEvents.PowerModeChanged -= OnPowerModeChanged;
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e) => QueueInteractionRecovery();

    private void QueueInteractionRecovery()
    {
        if (_disposed || _isExiting) return;
        _outsideClicks?.InvalidatePendingClicks();
        System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, new Action(() =>
        {
            if (_disposed || _isExiting) return;
            try { _outsideClicks?.Restart(); }
            catch (System.ComponentModel.Win32Exception exception) { _log.Error("Outside-click monitor could not be renewed.", exception); }
            RevalidateWindowPositions();
        }));
    }

    private void OnPowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
    {
        if (e.Mode == Microsoft.Win32.PowerModes.Suspend)
        {
            _outsideClicks?.InvalidatePendingClicks();
            _ = FlushBeforeSuspendAsync();
            return;
        }

        if (e.Mode == Microsoft.Win32.PowerModes.Resume)
        {
            QueueInteractionRecovery();
        }
    }

    private async Task FlushBeforeSuspendAsync()
    {
        try
        {
            await _saver.FlushAsync();
            _log.Information("State saved before system suspend.");
        }
        catch (Exception exception)
        {
            _log.Error("State could not be saved before system suspend.", exception);
        }
    }

    private void RevalidateWindowPositions()
    {
        if (_launcherWindow is not null)
        {
            OnLauncherPositionCommitted(_launcherWindow.Left, _launcherWindow.Top);
        }

        _windowManager.EnsureVisible(_state.TodoWindows);
        ScheduleSave();
    }

    private void CloseLauncherMenu()
    {
        if (_launcherViewModel is not null)
        {
            _launcherWindow?.DismissPanels();
            _launcherViewModel.ClosePanels();
        }
    }

    private async Task ExitAsync()
    {
        if (_isExiting)
        {
            return;
        }

        _isExiting = true;
        UnsubscribeFromSystemEvents();
        CloseLauncherMenu();

        try
        {
            await _saver.FlushAsync();
            await _saver.DisposeAsync();
        }
        catch (Exception exception)
        {
            _isExiting = false;
            SubscribeToSystemEvents();
            _log.Error("Application could not save state while closing.", exception);
            MessageBox.Show(
                $"No fue posible guardar los cambios. Todo's seguirá abierto.\n\n{exception.Message}",
                "No se pudo cerrar Todo's",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        _outsideClicks?.Dispose();
        _notificationArea?.Dispose();
        _recycleBinWindow?.Close();
        _windowManager.CloseAll();
        if (_launcherWindow is not null)
        {
            _launcherWindow.AllowClose = true;
            _launcherWindow.Close();
        }

        System.Windows.Application.Current.Shutdown();
    }
}
