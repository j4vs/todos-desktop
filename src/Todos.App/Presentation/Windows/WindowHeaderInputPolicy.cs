using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Todos.Presentation.Windows;

public static class WindowHeaderInputPolicy
{
    public static bool CanStartDrag(DependencyObject? source, FrameworkElement header, TextBox title)
    {
        for (var current = source; current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (ReferenceEquals(current, header)) return true;
            if (current is ButtonBase) return false;
            if (current is TextBox editor && (!ReferenceEquals(editor, title) || !title.IsReadOnly)) return false;
        }
        return false;
    }
}
