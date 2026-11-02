using System.Windows.Input;
using Todos.Presentation.ViewModels;

namespace Todos.Presentation.Views;

public partial class RecycleBinWindow : System.Windows.Window
{
    public RecycleBinWindow(RecycleBinViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();
    private void OnHeaderMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }
    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape) return;
        var viewModel = (RecycleBinViewModel)DataContext;
        if (viewModel.IsConfirmingDelete) viewModel.CancelDeleteCommand.Execute(null);
        else Close();
        e.Handled = true;
    }
}
