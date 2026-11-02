using System.Collections.ObjectModel;
using Todos.Domain;
using Todos.Domain.Models;
using Todos.Presentation.Mvvm;

namespace Todos.Presentation.ViewModels;

public sealed class TodoTaskViewModel : ObservableObject
{
    private readonly TodoTaskItem _item;
    private readonly Action<Guid, bool> _setCompleted;
    private readonly Action<TodoTaskViewModel> _delete;
    private readonly Func<Guid, string, bool> _saveEdit;
    private string _text;
    private bool _isCompleted;
    private bool _isEditing;
    private string _editText;
    private string _groupName = "General";
    private bool _isContextTarget;
    public bool IsContextTarget { get => _isContextTarget; set => SetProperty(ref _isContextTarget, value); }
    private string _notesSaveStatus = "Guardado";
    public string NotesSaveStatus { get => _notesSaveStatus; set => SetProperty(ref _notesSaveStatus, value); }
    public string NotesMarkdown
    {
        get => _item.NotesMarkdown;
        set
        {
            value ??= string.Empty;
            if (_item.NotesMarkdown == value) return;
            _item.NotesMarkdown = value;
            _item.UpdatedAt = DateTimeOffset.UtcNow;
            NotesSaveStatus = "Guardando…";
            OnPropertyChanged(); OnPropertyChanged(nameof(HasNotes)); OnPropertyChanged(nameof(NotesMenuLabel));
            _stateChanged?.Invoke();
        }
    }
    public bool HasNotes => !string.IsNullOrWhiteSpace(NotesMarkdown);
    public string NotesMenuLabel => HasNotes ? "Ver nota" : "Agregar nota";

    public TodoTaskViewModel(
        TodoTaskItem item,
        Action<Guid, bool> setCompleted,
        Action<TodoTaskViewModel> delete,
        Func<Guid, string, bool> saveEdit, Guid? parentId = null, Action? stateChanged = null, Action<Guid, string>? addSubtask = null)
    {
        _item = item;
        ParentId = parentId;
        _stateChanged = stateChanged;
        _addSubtask = addSubtask;
        AddSubtaskCommand = new RelayCommand(() =>
        {
            if (string.IsNullOrWhiteSpace(NewSubtaskText)) return;
            _addSubtask?.Invoke(Id, NewSubtaskText);
            NewSubtaskText = string.Empty;
        }, () => !string.IsNullOrWhiteSpace(NewSubtaskText));
        ToggleSubtasksCommand = new RelayCommand(() => AreSubtasksExpanded = !AreSubtasksExpanded);
        _setCompleted = setCompleted;
        _delete = delete;
        _saveEdit = saveEdit;
        _text = item.Text;
        _editText = item.Text;
        _isCompleted = item.IsCompleted;
        BeginEditCommand = new RelayCommand(BeginEdit);
        SaveEditCommand = new RelayCommand(() => SaveEdit());
        CancelEditCommand = new RelayCommand(CancelEdit);
        DeleteCommand = new RelayCommand(() => _delete(this));
    }

    private readonly Action? _stateChanged;
    private readonly Action<Guid, string>? _addSubtask;
    private string _newSubtaskText = string.Empty;
    private bool _isSubtaskEditorVisible;
    public Guid? ParentId { get; }
    public ObservableCollection<TodoTaskViewModel> Subtasks { get; } = [];
    public RelayCommand AddSubtaskCommand { get; }
    public RelayCommand ToggleSubtasksCommand { get; }
    public bool HasSubtasks => Subtasks.Count > 0;
    public bool HasSubtaskControls => HasSubtasks || IsSubtaskEditorVisible;
    public string SubtaskProgress => $"{Subtasks.Count(child => child.IsCompleted)}/{Subtasks.Count}";
    public bool AreSubtasksExpanded
    {
        get => _item.AreSubtasksExpanded;
        set { if (_item.AreSubtasksExpanded == value) return; _item.AreSubtasksExpanded = value; OnPropertyChanged(); _stateChanged?.Invoke(); }
    }
    public bool IsSubtaskEditorVisible
    {
        get => _isSubtaskEditorVisible;
        private set { if (SetProperty(ref _isSubtaskEditorVisible, value)) OnPropertyChanged(nameof(HasSubtaskControls)); }
    }
    public string NewSubtaskText
    {
        get => _newSubtaskText;
        set { if (SetProperty(ref _newSubtaskText, value.Length > AppConstants.MaxTaskTextLength ? value[..AppConstants.MaxTaskTextLength] : value)) AddSubtaskCommand.NotifyCanExecuteChanged(); }
    }
    public void BeginSubtaskCapture() { AreSubtasksExpanded = true; IsSubtaskEditorVisible = true; }
    public void EndSubtaskCapture() { IsSubtaskEditorVisible = false; NewSubtaskText = string.Empty; }
    public void RefreshSubtasks(IEnumerable<TodoTaskViewModel> children)
    {
        Subtasks.Clear();
        foreach (var child in children) Subtasks.Add(child);
        OnPropertyChanged(nameof(HasSubtasks)); OnPropertyChanged(nameof(HasSubtaskControls));
        OnPropertyChanged(nameof(SubtaskProgress)); OnPropertyChanged(nameof(AreSubtasksExpanded));
    }

    public Guid Id => _item.Id;

    public Guid? GroupId => _item.GroupId;
    public string GroupName { get => _groupName; set => SetProperty(ref _groupName, value); }

    public string Text
    {
        get => _text;
        private set => SetProperty(ref _text, value);
    }

    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            if (_isCompleted == value)
            {
                return;
            }

            _setCompleted(Id, value);
        }
    }

    public bool IsEditing
    {
        get => _isEditing;
        private set => SetProperty(ref _isEditing, value);
    }

    public string EditText
    {
        get => _editText;
        set => SetProperty(ref _editText, value);
    }

    public RelayCommand BeginEditCommand { get; }

    public RelayCommand SaveEditCommand { get; }

    public RelayCommand CancelEditCommand { get; }

    public RelayCommand DeleteCommand { get; }

    public void SynchronizeFromModel()
    {
        Text = _item.Text;
        _isCompleted = _item.IsCompleted;
        OnPropertyChanged(nameof(IsCompleted));
        if (!IsEditing)
        {
            EditText = Text;
        }
    }

    public bool SaveEdit()
    {
        if (!_saveEdit(Id, EditText))
        {
            return false;
        }

        Text = _item.Text;
        EditText = Text;
        IsEditing = false;
        return true;
    }

    public void CancelEdit()
    {
        EditText = Text;
        IsEditing = false;
    }

    private void BeginEdit()
    {
        EditText = Text;
        IsEditing = true;
    }
}
