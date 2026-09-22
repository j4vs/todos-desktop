using System.Collections.ObjectModel;
using Todos.Domain;
using Todos.Domain.Models;
using Todos.Presentation.Mvvm;

namespace Todos.Presentation.ViewModels;

public sealed class TodoWindowViewModel : ObservableObject, IDisposable
{
    private readonly TodoWindowState _state;
    private readonly Action _stateChanged;
    private string _title;
    private TodoColorTheme _colorTheme;
    private string _newTaskText = string.Empty;
    private bool _areCompletedTasksExpanded;
    private bool _isUndoVisible;
    private string _undoMessage = string.Empty;
    private TodoTaskItem? _lastDeletedTask;
    private CancellationTokenSource? _undoCancellation;
    private TodoGroupViewModel? _selectedGroup;
    private string _newGroupName = string.Empty;
    private string _groupError = string.Empty;

    public TodoWindowViewModel(
        TodoWindowState state,
        Action stateChanged,
        Action hide,
        Action requestDelete)
    {
        _state = state;
        _stateChanged = stateChanged;
        _title = state.Title;
        _colorTheme = state.ColorTheme;
        _areCompletedTasksExpanded = state.AreCompletedTasksExpanded;
        HideCommand = new RelayCommand(hide);
        DeleteCommand = new RelayCommand(requestDelete);
        SelectThemeCommand = new RelayCommand<TodoColorTheme>(SelectTheme);
        AddTaskCommand = new RelayCommand(AddTask, CanAddTask);
        ToggleCompletedSectionCommand = new RelayCommand(ToggleCompletedSection);
        UndoDeleteCommand = new RelayCommand(UndoDelete, () => IsUndoVisible);
        AddGroupCommand = new RelayCommand(AddGroup, () => !string.IsNullOrWhiteSpace(NewGroupName));
        RefreshTasks();
    }

    public Guid Id => _state.Id;

    public ObservableCollection<TodoGroupViewModel> Groups { get; } = [];
    public RelayCommand AddGroupCommand { get; }
    public TodoGroupViewModel? SelectedGroup
    {
        get => _selectedGroup;
        set => SetProperty(ref _selectedGroup, value);
    }

    public string NewGroupName
    {
        get => _newGroupName;
        set
        {
            if (SetProperty(ref _newGroupName, value)) AddGroupCommand.NotifyCanExecuteChanged();
        }
    }

    public string GroupError { get => _groupError; private set => SetProperty(ref _groupError, value); }

    private void AddGroup()
    {
        try
        {
            var group = TodoListOperations.AddGroup(_state, NewGroupName, DateTimeOffset.UtcNow);
            GroupError = string.Empty;
            NewGroupName = string.Empty;
            RefreshTasks();
            SelectedGroup = Groups.Single(item => item.Id == group.Id);
            _stateChanged();
        }
        catch (ArgumentException exception) { GroupError = exception.Message.Split(Environment.NewLine)[0]; }
    }

    public void RenameGroup(TodoGroupViewModel group)
    {
        if (group.Id is not { } id) return;
        try
        {
            TodoListOperations.RenameGroup(_state, id, group.Name, DateTimeOffset.UtcNow);
            GroupError = string.Empty;
            group.Name = _state.Groups.Single(item => item.Id == id).Name;
            foreach (var task in PendingTasks.Concat(CompletedTasks).Where(task => task.GroupId == id)) task.GroupName = group.Name;
            _stateChanged();
        }
        catch (ArgumentException exception)
        {
            GroupError = exception.Message.Split(Environment.NewLine)[0];
            group.Name = _state.Groups.Single(item => item.Id == id).Name;
        }
    }

    public void RemoveGroup(TodoGroupViewModel group)
    {
        if (group.Id is not { } id) return;
        TodoListOperations.RemoveGroup(_state, id, DateTimeOffset.UtcNow);
        RefreshTasks();
        _stateChanged();
    }

    public void MoveTaskToGroup(Guid taskId, Guid? groupId, int destinationIndex = int.MaxValue)
    {
        TodoListOperations.MoveTaskToGroup(_state, taskId, groupId, destinationIndex, DateTimeOffset.UtcNow);
        RefreshTasks();
        _stateChanged();
    }

    public string Title
    {
        get => _title;
        set
        {
            var limited = value.Length <= AppConstants.MaxTitleLength
                ? value
                : value[..AppConstants.MaxTitleLength];
            if (!SetProperty(ref _title, limited))
            {
                return;
            }

            _state.Title = limited;
            TouchAndSave();
        }
    }

    public TodoColorTheme ColorTheme
    {
        get => _colorTheme;
        private set
        {
            if (SetProperty(ref _colorTheme, value))
            {
                _state.ColorTheme = value;
                TouchAndSave();
            }
        }
    }

    public string NewTaskText
    {
        get => _newTaskText;
        set
        {
            var limited = value.Length <= AppConstants.MaxTaskTextLength
                ? value
                : value[..AppConstants.MaxTaskTextLength];
            if (SetProperty(ref _newTaskText, limited))
            {
                AddTaskCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool AreCompletedTasksExpanded
    {
        get => _areCompletedTasksExpanded;
        set
        {
            if (!SetProperty(ref _areCompletedTasksExpanded, value))
            {
                return;
            }

            _state.AreCompletedTasksExpanded = value;
            TouchAndSave();
        }
    }

    public bool IsUndoVisible
    {
        get => _isUndoVisible;
        private set
        {
            if (SetProperty(ref _isUndoVisible, value))
            {
                UndoDeleteCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string UndoMessage
    {
        get => _undoMessage;
        private set => SetProperty(ref _undoMessage, value);
    }

    public int PendingCount => PendingTasks.Count;

    public int CompletedCount => CompletedTasks.Count;

    public bool HasAnyTasks => PendingCount + CompletedCount > 0;

    public bool HasCompletedTasks => CompletedCount > 0;

    public string Summary => $"{PendingCount} pendientes · {CompletedCount} completadas";

    public string CompletedHeader => $"Completadas ({CompletedCount})";

    public ObservableCollection<TodoTaskViewModel> PendingTasks { get; } = [];

    public ObservableCollection<TodoTaskViewModel> CompletedTasks { get; } = [];

    public RelayCommand HideCommand { get; }

    public RelayCommand DeleteCommand { get; }

    public RelayCommand<TodoColorTheme> SelectThemeCommand { get; }

    public RelayCommand AddTaskCommand { get; }

    public RelayCommand ToggleCompletedSectionCommand { get; }

    public RelayCommand UndoDeleteCommand { get; }

    public void CommitTitle()
    {
        Title = TodoText.NormalizeTitle(Title);
        _state.Title = Title;
        TouchAndSave();
    }

    public void CommitBounds(double left, double top, double width, double height)
    {
        _state.PositionX = left;
        _state.PositionY = top;
        _state.Width = Math.Max(width, AppConstants.MinimumWindowWidth);
        _state.Height = Math.Max(height, AppConstants.MinimumWindowHeight);
        TouchAndSave();
    }

    public void MovePendingTask(Guid taskId, int destinationIndex)
    {
        TodoListOperations.MovePendingTask(_state, taskId, destinationIndex, DateTimeOffset.UtcNow);
        RefreshTasks();
        _stateChanged();
    }

    public void Dispose()
    {
        _undoCancellation?.Cancel();
        _undoCancellation?.Dispose();
    }

    private bool CanAddTask() => !string.IsNullOrWhiteSpace(NewTaskText);

    private void AddTask()
    {
        if (!CanAddTask())
        {
            return;
        }

        TodoListOperations.AddTask(_state, NewTaskText, DateTimeOffset.UtcNow, SelectedGroup?.Id);
        NewTaskText = string.Empty;
        RefreshTasks();
        _stateChanged();
    }

    private void SetCompleted(Guid taskId, bool completed)
    {
        if (completed)
        {
            TodoListOperations.CompleteTask(_state, taskId, DateTimeOffset.UtcNow);
        }
        else
        {
            TodoListOperations.ReopenTask(_state, taskId, DateTimeOffset.UtcNow);
        }

        RefreshTasks();
        _stateChanged();
    }

    private bool SaveTaskEdit(Guid taskId, string text)
    {
        try
        {
            TodoListOperations.EditTask(_state, taskId, text, DateTimeOffset.UtcNow);
            RefreshTasks();
            _stateChanged();
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private void DeleteTask(TodoTaskViewModel taskViewModel)
    {
        _undoCancellation?.Cancel();
        _undoCancellation?.Dispose();
        _lastDeletedTask = TodoListOperations.RemoveTask(_state, taskViewModel.Id, DateTimeOffset.UtcNow);
        UndoMessage = $"“{Shorten(_lastDeletedTask.Text, 36)}” eliminada";
        IsUndoVisible = true;
        RefreshTasks();
        _stateChanged();

        _undoCancellation = new CancellationTokenSource();
        _ = HideUndoAfterDelayAsync(_undoCancellation.Token);
    }

    private void UndoDelete()
    {
        if (_lastDeletedTask is null)
        {
            return;
        }

        _undoCancellation?.Cancel();
        TodoListOperations.RestoreTask(_state, _lastDeletedTask, DateTimeOffset.UtcNow);
        _lastDeletedTask = null;
        IsUndoVisible = false;
        RefreshTasks();
        _stateChanged();
    }

    private async Task HideUndoAfterDelayAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(AppConstants.UndoDeleteDuration, cancellationToken);
            _lastDeletedTask = null;
            IsUndoVisible = false;
        }
        catch (OperationCanceledException)
        {
            // A new deletion or undo replaced this timeout.
        }
    }

    private void ToggleCompletedSection() => AreCompletedTasksExpanded = !AreCompletedTasksExpanded;

    private void SelectTheme(TodoColorTheme theme) => ColorTheme = theme;

    private void RefreshTasks()
    {
        var existing = PendingTasks.Concat(CompletedTasks).ToDictionary(task => task.Id);
        PendingTasks.Clear();
        CompletedTasks.Clear();

        foreach (var item in TodoListOperations.GetPending(_state))
        {
            var viewModel = GetOrCreateTaskViewModel(existing, item);
            viewModel.SynchronizeFromModel();
            PendingTasks.Add(viewModel);
        }

        foreach (var item in TodoListOperations.GetCompleted(_state))
        {
            var viewModel = GetOrCreateTaskViewModel(existing, item);
            viewModel.SynchronizeFromModel();
            CompletedTasks.Add(viewModel);
        }

        var selectedId = SelectedGroup?.Id;
        if (Groups.Count == 0) Groups.Add(new TodoGroupViewModel(null, "General", _state.IsGeneralGroupExpanded,
            expanded => { _state.IsGeneralGroupExpanded = expanded; TouchAndSave(); }));
        foreach (var removed in Groups.Where(group => group.Id.HasValue && !_state.Groups.Any(item => item.Id == group.Id)).ToArray())
            Groups.Remove(removed);
        foreach (var group in _state.Groups)
        {
            if (!Groups.Any(item => item.Id == group.Id)) Groups.Add(new TodoGroupViewModel(group.Id, group.Name, group.IsExpanded,
                expanded => { group.IsExpanded = expanded; TouchAndSave(); }));
        }
        foreach (var group in Groups) group.Refresh(PendingTasks.Where(task => task.GroupId == group.Id));
        foreach (var task in PendingTasks.Concat(CompletedTasks)) task.GroupName = Groups.Single(group => group.Id == task.GroupId).Name;
        SelectedGroup = Groups.FirstOrDefault(group => group.Id == selectedId) ?? Groups[0];

        OnPropertyChanged(nameof(PendingCount));
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(HasAnyTasks));
        OnPropertyChanged(nameof(HasCompletedTasks));
        OnPropertyChanged(nameof(Summary));
        OnPropertyChanged(nameof(CompletedHeader));
    }

    private TodoTaskViewModel GetOrCreateTaskViewModel(
        IReadOnlyDictionary<Guid, TodoTaskViewModel> existing,
        TodoTaskItem item) =>
        existing.TryGetValue(item.Id, out var viewModel)
            ? viewModel
            : new TodoTaskViewModel(item, SetCompleted, DeleteTask, SaveTaskEdit);

    private void TouchAndSave()
    {
        _state.UpdatedAt = DateTimeOffset.UtcNow;
        _stateChanged();
    }

    private static string Shorten(string value, int maximumLength) =>
        value.Length <= maximumLength ? value : $"{value[..(maximumLength - 1)]}…";
}
