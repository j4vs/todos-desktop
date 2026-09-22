using System.Collections.ObjectModel;
using Todos.Presentation.Mvvm;

namespace Todos.Presentation.ViewModels;

public sealed class TodoGroupViewModel(Guid? id, string name, bool isExpanded = true, Action<bool>? expandedChanged = null) : ObservableObject
{
    private string _name = name;
    private bool _isExpanded = isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (SetProperty(ref _isExpanded, value)) expandedChanged?.Invoke(value);
        }
    }
    public Guid? Id { get; } = id;
    public bool IsGeneral => Id is null;
    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public ObservableCollection<TodoTaskViewModel> Tasks { get; } = [];
    public bool IsEmpty => Tasks.Count == 0;
    public string CountLabel => $"{Tasks.Count}";

    public void Refresh(IEnumerable<TodoTaskViewModel> tasks)
    {
        Tasks.Clear();
        foreach (var task in tasks) Tasks.Add(task);
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(CountLabel));
    }
}
