using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Todos.Presentation.Windows;

public static class ScreenInteraction
{
    public static bool Contains(FrameworkElement? element, System.Windows.Point screenPoint)
    {
        if (element is null || !element.IsVisible || !element.IsLoaded) return false;
        if (!double.IsFinite(screenPoint.X) || !double.IsFinite(screenPoint.Y) ||
            screenPoint.X < int.MinValue || screenPoint.X > int.MaxValue || screenPoint.Y < int.MinValue || screenPoint.Y > int.MaxValue ||
            PresentationSource.FromVisual(element) is not HwndSource { IsDisposed: false } source) return false;
        // The low-level mouse hook reports physical, per-monitor screen coordinates.
        // Compare HWNDs and native bounds; PointFromScreen can use a different DPI context
        // after a display change or while a native window is dispatching input.
        var clicked = WindowFromPhysicalPoint(new NativePoint((int)screenPoint.X, (int)screenPoint.Y));
        if (clicked != nint.Zero && GetAncestor(clicked, 2) == GetAncestor(source.Handle, 2)) return true;
        // Keep transparent gaps in a popup usable, as in the original geometric
        // hit test, but request physical bounds instead of a WPF/DPI conversion.
        var previousContext = SetThreadDpiAwarenessContext(new nint(-3));
        try
        {
            return GetWindowRect(source.Handle, out var bounds) && screenPoint.X >= bounds.Left && screenPoint.X < bounds.Right &&
                screenPoint.Y >= bounds.Top && screenPoint.Y < bounds.Bottom;
        }
        finally { if (previousContext != nint.Zero) SetThreadDpiAwarenessContext(previousContext); }
    }

    [StructLayout(LayoutKind.Sequential)] private readonly record struct NativePoint(int X, int Y);
    [DllImport("user32.dll")] private static extern nint WindowFromPhysicalPoint(NativePoint point);
    [DllImport("user32.dll")] private static extern nint GetAncestor(nint window, uint flags);
    [StructLayout(LayoutKind.Sequential)] private struct NativeRect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetWindowRect(nint window, out NativeRect bounds);
    [DllImport("user32.dll")] private static extern nint SetThreadDpiAwarenessContext(nint context);
}
