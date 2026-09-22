using System.Windows.Media;

namespace Todos.Presentation.Windows;

public static class TaskDragVisualPolicy
{
    public static ScaleTransform EnsureMutableScaleTransform(FrameworkElement element)
    {
        var transform = element.RenderTransform as ScaleTransform;
        if (transform is null)
        {
            transform = new ScaleTransform(1, 1);
            element.RenderTransform = transform;
        }
        else if (transform.IsFrozen)
        {
            transform = transform.Clone();
            element.RenderTransform = transform;
        }

        return transform;
    }

    public static TranslateTransform EnsureMutableTranslateTransform(FrameworkElement element)
    {
        var transform = element.RenderTransform as TranslateTransform;
        if (transform is null)
        {
            transform = new TranslateTransform();
            element.RenderTransform = transform;
        }
        else if (transform.IsFrozen)
        {
            transform = transform.Clone();
            element.RenderTransform = transform;
        }

        return transform;
    }
}
