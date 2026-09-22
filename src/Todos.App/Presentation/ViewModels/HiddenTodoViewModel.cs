using Todos.Domain.Models;
using Todos.Presentation.Mvvm;

namespace Todos.Presentation.ViewModels;

public sealed class HiddenTodoViewModel
{
    public HiddenTodoViewModel(TodoWindowState state, Action<Guid> show, Action<Guid> delete)
    {
        Id = state.Id;
        Title = state.Title;
        ColorTheme = state.ColorTheme;
        ShowCommand = new RelayCommand(() => show(Id));
        DeleteCommand = new RelayCommand(() => delete(Id));
    }

    public Guid Id { get; }

    public string Title { get; }

    public TodoColorTheme ColorTheme { get; }

    public RelayCommand ShowCommand { get; }

    public RelayCommand DeleteCommand { get; }
}
