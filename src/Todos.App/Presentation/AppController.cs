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
            ExitAsync)
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
            AppConstants.LauncherSize,
            AppConstants.LauncherSize,
            area);
        _launcherWindow.Left = position.X;
        _launcherWindow.Top = position.Y;

        System.Windows.Application.Current.MainWindow = _launcherWindow;
        _launcherWindow.Show();
        _notificationArea = new NotificationAreaService(ShowLauncher, CreateList, RequestExit);

        foreach (var todoState in _state.TodoWindows.Where(window => window.IsVisible))
        {
            _windowManager.Show(todoState);
        }

        RefreshHiddenWindows();

        if (_launchOptions.RenderLauncherPreviewPath is not null)
        {
            _launcherViewModel.IsHiddenSubmenuOpen = _launcherViewModel.HasHiddenWindows;
            _launcherViewModel.IsMenuOpen = true;
            await Task.Delay(350);
            await WindowPreviewRenderer.RenderPngAsync(
                _launcherWindow.MenuPreviewContent,
                _launchOptions.RenderLauncherPreviewPath);
            var previewDirectory = Path.GetDirectoryName(_launchOptions.RenderLauncherPreviewPath) ?? string.Empty;
            var previewName = Path.GetFileNameWithoutExtension(_launchOptions.RenderLauncherPreviewPath);
            var previewExtension = Path.GetExtension(_launchOptions.RenderLauncherPreviewPath);
            var hiddenPreviewPath = Path.Combine(previewDirectory, $"{previewName}-hidden{previewExtension}");
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
        UnsubscribeFromSystemEvents();
        _notificationArea?.Dispose();
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
        var result = MessageBox.Show(
            $"¿Quieres eliminar la lista “{TodoText.NormalizeTitle(state.Title)}”?\n\nEsta acción no tiene papelera.",
            "Eliminar lista",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            System.Windows.MessageBoxResult.No);
        if (result != System.Windows.MessageBoxResult.Yes)
        {
            return;
        }

        _windowManager.Remove(state);
        _state.TodoWindows.Remove(state);
        RefreshHiddenWindows();
        ScheduleSave();
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
            AppConstants.LauncherSize,
            AppConstants.LauncherSize,
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

    private void RefreshHiddenWindows() => _launcherViewModel?.RefreshHiddenWindows(_state.TodoWindows);

    private void ScheduleSave() => _saver.Schedule(_state);

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
            _launchOptions.RenderLauncherPreviewPath is not null)
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

    private void OnDisplaySettingsChanged(object? sender, EventArgs e) =>
        System.Windows.Application.Current.Dispatcher.BeginInvoke(RevalidateWindowPositions);

    private void OnPowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
    {
        if (e.Mode == Microsoft.Win32.PowerModes.Suspend)
        {
            _ = FlushBeforeSuspendAsync();
            return;
        }

        if (e.Mode == Microsoft.Win32.PowerModes.Resume)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(RevalidateWindowPositions);
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
            _launcherViewModel.IsMenuOpen = false;
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

        _notificationArea?.Dispose();
        _windowManager.CloseAll();
        if (_launcherWindow is not null)
        {
            _launcherWindow.AllowClose = true;
            _launcherWindow.Close();
        }

        System.Windows.Application.Current.Shutdown();
    }
}
