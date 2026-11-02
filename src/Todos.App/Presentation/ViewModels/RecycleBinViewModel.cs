using System.Collections.ObjectModel;
using Todos.Domain.Models;
using Todos.Presentation.Mvvm;

namespace Todos.Presentation.ViewModels;

public sealed class RecycleBinViewModel : ObservableObject
{
    private readonly Action<Guid> _restore;
    private readonly Action<Guid> _delete;
    private TrashedListViewModel? _pendingDeletion;

    public RecycleBinViewModel(Action<Guid> restore, Action<Guid> delete)
    {
        _restore = restore;
        _delete = delete;
        CancelDeleteCommand = new RelayCommand(() => PendingDeletion = null);
        ConfirmDeleteCommand = new RelayCommand(() =>
        {
            if (PendingDeletion is not { } pending) return;
            PendingDeletion = null;
            _delete(pending.Id);
        }, () => PendingDeletion is not null);
    }

    public ObservableCollection<TrashedListViewModel> Lists { get; } = [];
    public bool IsEmpty => Lists.Count == 0;
    public string Summary => Lists.Count == 1 ? "1 lista en papelera" : $"{Lists.Count} listas en papelera";
    public bool IsConfirmingDelete => PendingDeletion is not null;
    public TrashedListViewModel? PendingDeletion
    {
        get => _pendingDeletion;
        private set
        {
            if (!SetProperty(ref _pendingDeletion, value)) return;
            OnPropertyChanged(nameof(IsConfirmingDelete));
            ConfirmDeleteCommand.NotifyCanExecuteChanged();
        }
    }
    public RelayCommand CancelDeleteCommand { get; }
    public RelayCommand ConfirmDeleteCommand { get; }

    public void Refresh(IEnumerable<TodoWindowState> windows)
    {
        var states = windows.OrderByDescending(window => window.TrashedAt).ThenBy(window => window.Title).ToArray();
        if (PendingDeletion is { } pending && !states.Any(window => window.Id == pending.Id)) PendingDeletion = null;
        Lists.Clear();
        foreach (var window in states)
            Lists.Add(new TrashedListViewModel(window, _restore, item => PendingDeletion = item));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(Summary));
    }
}

public sealed class TrashedListViewModel
{
    public TrashedListViewModel(TodoWindowState window, Action<Guid> restore, Action<TrashedListViewModel> requestDelete)
    {
        Id = window.Id;
        Title = window.Title;
        ColorTheme = window.ColorTheme;
        var completed = window.Tasks.Count(task => task.IsCompleted);
        Details = $"{window.Tasks.Count - completed} pendientes · {completed} completadas";
        RestoreCommand = new RelayCommand(() => restore(Id));
        DeleteCommand = new RelayCommand(() => requestDelete(this));
    }
    public Guid Id { get; }
    public string Title { get; }
    public TodoColorTheme ColorTheme { get; }
    public string Details { get; }
    public RelayCommand RestoreCommand { get; }
    public RelayCommand DeleteCommand { get; }
}
