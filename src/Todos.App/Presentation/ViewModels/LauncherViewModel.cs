using System.Collections.ObjectModel;
using Todos.Domain.Models;
using Todos.Presentation.Mvvm;
using Todos.Presentation.Themes;

namespace Todos.Presentation.ViewModels;

public sealed class LauncherViewModel : ObservableObject
{
    private bool _isMenuOpen;
    private bool _isSettingsOpen;
    private bool _autoMinimizeWindows;
    private Guid? _associatedWindowId;
    private TodoWindowState? _associatedState;
    private readonly Action<Guid>? _hideAssociated;
    private string _launcherTitle = "Todo’s";
    private string _launcherColor = "#9285D8";
    private readonly Action<bool>? _changeAutoMinimizePreference;
    private bool _runAtWindowsStartup;
    private bool _isHiddenSubmenuOpen;
    private string? _statusMessage;
    private readonly Action<Guid> _showHidden;
    private readonly Action<Guid> _deleteHidden;
    private readonly Func<bool, bool> _changeStartupPreference;

    public LauncherViewModel(
        bool runAtWindowsStartup,
        Action createList,
        Action showAll,
        Action hideAll,
        Action<Guid> showHidden,
        Action<Guid> deleteHidden,
        Func<bool, bool> changeStartupPreference,
        Func<Task> exit,
        bool autoMinimizeWindows = false,
        Action<bool>? changeAutoMinimizePreference = null,
        Action<Guid>? hideAssociated = null,
        Action? showTrash = null,
        Action? hideLauncher = null,
        AppSettings? appearanceSettings = null, Action? appearanceChanged = null)
    {
        _appearanceSettings = appearanceSettings ?? new AppSettings();
        _appearanceChanged = appearanceChanged;
        SmallerFontCommand = new RelayCommand(() => ChangeFont(-1), () => _appearanceSettings.FontSizeStep > -2);
        LargerFontCommand = new RelayCommand(() => ChangeFont(1), () => _appearanceSettings.FontSizeStep < 4);
        _runAtWindowsStartup = runAtWindowsStartup;
        _autoMinimizeWindows = autoMinimizeWindows;
        _hideAssociated = hideAssociated;
        _changeAutoMinimizePreference = changeAutoMinimizePreference;
        _showHidden = showHidden;
        _deleteHidden = deleteHidden;
        _changeStartupPreference = changeStartupPreference;
        CreateListCommand = new RelayCommand(createList);
        ShowAllCommand = new RelayCommand(showAll);
        HideAllCommand = new RelayCommand(hideAll);
        ToggleHiddenSubmenuCommand = new RelayCommand(
            () => IsHiddenSubmenuOpen = !IsHiddenSubmenuOpen,
            () => HasHiddenWindows);
        ExitCommand = new AsyncRelayCommand(exit);
        MinimizeCommand = new RelayCommand(() => { ClosePanels(); hideLauncher?.Invoke(); });
        ShowTrashCommand = new RelayCommand(() => { ClosePanels(); showTrash?.Invoke(); });
    }

    private readonly AppSettings _appearanceSettings;
    private readonly Action? _appearanceChanged;
    public RelayCommand SmallerFontCommand { get; }
    public RelayCommand LargerFontCommand { get; }
    public string FontSizeLabel => $"{100 + _appearanceSettings.FontSizeStep * 10} %";
    public bool LauncherSmall { get => _appearanceSettings.LauncherSize == 0; set { if (value) SetLauncherSize(0); } }
    public bool LauncherMedium { get => _appearanceSettings.LauncherSize == 1; set { if (value) SetLauncherSize(1); } }
    public bool LauncherLarge { get => _appearanceSettings.LauncherSize == 2; set { if (value) SetLauncherSize(2); } }
    public bool SpacingSmall { get => _appearanceSettings.SpacingSize == 0; set { if (value) SetSpacingSize(0); } }
    public bool SpacingMedium { get => _appearanceSettings.SpacingSize == 1; set { if (value) SetSpacingSize(1); } }
    public bool SpacingLarge { get => _appearanceSettings.SpacingSize == 2; set { if (value) SetSpacingSize(2); } }

    private void ChangeFont(int direction)
    {
        _appearanceSettings.FontSizeStep = Math.Clamp(_appearanceSettings.FontSizeStep + direction, -2, 4);
        OnPropertyChanged(nameof(FontSizeLabel));
        SmallerFontCommand.NotifyCanExecuteChanged();
        LargerFontCommand.NotifyCanExecuteChanged();
        _appearanceChanged?.Invoke();
    }
    private void SetLauncherSize(int size)
    {
        if (_appearanceSettings.LauncherSize == size) return;
        _appearanceSettings.LauncherSize = size;
        OnPropertyChanged(nameof(LauncherSmall)); OnPropertyChanged(nameof(LauncherMedium)); OnPropertyChanged(nameof(LauncherLarge));
        _appearanceChanged?.Invoke();
    }
    private void SetSpacingSize(int size)
    {
        if (_appearanceSettings.SpacingSize == size) return;
        _appearanceSettings.SpacingSize = size;
        OnPropertyChanged(nameof(SpacingSmall)); OnPropertyChanged(nameof(SpacingMedium)); OnPropertyChanged(nameof(SpacingLarge));
        _appearanceChanged?.Invoke();
    }

    public bool IsMenuOpen
    {
        get => _isMenuOpen;
        set
        {
            if (!SetProperty(ref _isMenuOpen, value)) return;
            IsHiddenSubmenuOpen = false;
            IsSettingsOpen = false;
            OnPropertyChanged(nameof(IsPanelOpen));
        }
    }

    public bool IsSettingsOpen
    {
        get => _isSettingsOpen;
        set
        {
            if (value) { IsMenuOpen = false; IsHiddenSubmenuOpen = false; }
            SetProperty(ref _isSettingsOpen, value);
        }
    }

    public bool AutoMinimizeWindows
    {
        get => _autoMinimizeWindows;
        set
        {
            if (SetProperty(ref _autoMinimizeWindows, value))
                _changeAutoMinimizePreference?.Invoke(value);
        }
    }

    public bool IsPanelOpen => IsMenuOpen || IsHiddenSubmenuOpen;
    public Guid? AssociatedWindowId => _associatedWindowId;
    public string LauncherTitle => _launcherTitle;
    public string LauncherColor => _launcherColor;

    public void RefreshAssociation(IEnumerable<TodoWindowState> states, Guid? id)
    {
        var associated = states.FirstOrDefault(window => window.Id == id);
        _associatedState = associated;
        _associatedWindowId = associated?.Id;
        _launcherTitle = associated?.Title ?? "Todo’s";
        _launcherColor = associated is null ? "#9285D8" : TodoThemePalette.GetLauncherBackground(associated.ColorTheme);
        OnPropertyChanged(nameof(AssociatedWindowId));
        OnPropertyChanged(nameof(LauncherTitle));
        OnPropertyChanged(nameof(LauncherColor));
    }

    public void PrimaryClick() => PreparePrimaryClick()();

    public Action PreparePrimaryClick()
    {
        // Decide on press, before the global outside-click handler can hide the list.
        var id = _associatedWindowId;
        var shouldHide = AutoMinimizeWindows && _associatedState?.IsVisible == true && _hideAssociated is not null;
        return () =>
        {
            ClosePanels();
            if (id is { } target)
            {
                if (shouldHide) _hideAssociated!(target);
                else _showHidden(target);
            }
            else ShowHiddenLists();
        };
    }

    public void ShowHiddenLists()
    {
        if (IsMenuOpen || IsSettingsOpen) return;
        IsHiddenSubmenuOpen = HasHiddenWindows;
    }

    public void ClosePanels()
    {
        IsMenuOpen = false;
        IsSettingsOpen = false;
        IsHiddenSubmenuOpen = false;
    }

    public bool RunAtWindowsStartup
    {
        get => _runAtWindowsStartup;
        set
        {
            if (_runAtWindowsStartup == value || !_changeStartupPreference(value))
            {
                return;
            }

            SetProperty(ref _runAtWindowsStartup, value);
        }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsHiddenSubmenuOpen
    {
        get => _isHiddenSubmenuOpen;
        set
        {
            if (value && IsMenuOpen) return;
            if (SetProperty(ref _isHiddenSubmenuOpen, value)) OnPropertyChanged(nameof(IsPanelOpen));
        }
    }

    public RelayCommand MinimizeCommand { get; }

    public RelayCommand ShowTrashCommand { get; }

    public RelayCommand CreateListCommand { get; }

    public RelayCommand ShowAllCommand { get; }

    public RelayCommand HideAllCommand { get; }

    public RelayCommand ToggleHiddenSubmenuCommand { get; }

    public AsyncRelayCommand ExitCommand { get; }

    public ObservableCollection<HiddenTodoViewModel> HiddenWindows { get; } = [];

    public bool HasHiddenWindows => HiddenWindows.Count > 0;

    public string HiddenTitle => $"Listas ocultas ({HiddenWindows.Count})";

    public void ToggleMenu() => IsMenuOpen = !IsMenuOpen;

    public void RefreshHiddenWindows(IEnumerable<TodoWindowState> states)
    {
        HiddenWindows.Clear();
        foreach (var state in states.Where(state => !state.IsVisible).OrderBy(state => state.Title))
        {
            HiddenWindows.Add(new HiddenTodoViewModel(state, _showHidden, _deleteHidden));
        }

        if (!HasHiddenWindows)
        {
            IsHiddenSubmenuOpen = false;
        }

        OnPropertyChanged(nameof(HasHiddenWindows));
        OnPropertyChanged(nameof(HiddenTitle));
        ToggleHiddenSubmenuCommand.NotifyCanExecuteChanged();
    }
}
