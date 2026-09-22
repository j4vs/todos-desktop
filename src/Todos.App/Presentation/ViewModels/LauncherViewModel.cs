using System.Collections.ObjectModel;
using Todos.Domain.Models;
using Todos.Presentation.Mvvm;

namespace Todos.Presentation.ViewModels;

public sealed class LauncherViewModel : ObservableObject
{
    private bool _isMenuOpen;
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
        Func<Task> exit)
    {
        _runAtWindowsStartup = runAtWindowsStartup;
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
    }

    public bool IsMenuOpen
    {
        get => _isMenuOpen;
        set
        {
            if (SetProperty(ref _isMenuOpen, value) && !value)
            {
                IsHiddenSubmenuOpen = false;
            }
        }
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
        set => SetProperty(ref _isHiddenSubmenuOpen, value);
    }

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
