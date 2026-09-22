using System.Windows.Media;

namespace Todos.Presentation.Windows;

public static class LauncherInputPolicy
{
    public static bool IsInside(DependencyObject surface, DependencyObject? source)
    {
        ArgumentNullException.ThrowIfNull(surface);
        while (source is not null)
        {
            if (ReferenceEquals(source, surface))
            {
                return true;
            }

            source = source is Visual or System.Windows.Media.Media3D.Visual3D
                ? VisualTreeHelper.GetParent(source)
                : System.Windows.LogicalTreeHelper.GetParent(source);
        }

        return false;
    }
}
