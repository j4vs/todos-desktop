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

    public TodoTaskViewModel(
        TodoTaskItem item,
        Action<Guid, bool> setCompleted,
        Action<TodoTaskViewModel> delete,
        Func<Guid, string, bool> saveEdit)
    {
        _item = item;
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
