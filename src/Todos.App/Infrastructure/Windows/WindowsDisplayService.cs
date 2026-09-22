using System.Runtime.InteropServices;
using Todos.Application.Windows;

namespace Todos.Infrastructure.Windows;

public sealed class WindowsDisplayService : IDisplayService
{
    public DisplayArea GetBestWorkingArea(double? x, double? y)
    {
        var areas = System.Windows.Forms.Screen.AllScreens
            .Select(screen => new ScreenArea(screen, ConvertWorkingArea(screen)))
            .ToArray();
        if (areas.Length == 0)
        {
            return new DisplayArea(0, 0, 1920, 1080);
        }

        if (x.HasValue && y.HasValue && double.IsFinite(x.Value) && double.IsFinite(y.Value))
        {
            var containing = areas.FirstOrDefault(item => Contains(item.Area, x.Value, y.Value));
            if (containing is not null)
            {
                return containing.Area;
            }

            return areas.MinBy(item => DistanceSquared(item.Area, x.Value, y.Value))!.Area;
        }

        return areas.FirstOrDefault(item => item.Screen.Primary)?.Area ?? areas[0].Area;
    }

    private static DisplayArea ConvertWorkingArea(System.Windows.Forms.Screen screen)
    {
        var scale = GetDpiForScreen(screen) / 96d;
        var area = screen.WorkingArea;
        return new DisplayArea(
            area.Left / scale,
            area.Top / scale,
            area.Width / scale,
            area.Height / scale);
    }

    private static uint GetDpiForScreen(System.Windows.Forms.Screen screen)
    {
        try
        {
            var center = new NativePoint(
                screen.Bounds.Left + (screen.Bounds.Width / 2),
                screen.Bounds.Top + (screen.Bounds.Height / 2));
            var monitor = MonitorFromPoint(center, 2);
            if (monitor != IntPtr.Zero && GetDpiForMonitor(monitor, 0, out var dpiX, out _) == 0)
            {
                return dpiX;
            }
        }
        catch (DllNotFoundException)
        {
        }
        catch (EntryPointNotFoundException)
        {
        }

        try
        {
            return GetDpiForSystem();
        }
        catch (EntryPointNotFoundException)
        {
            return 96;
        }
    }

    private static bool Contains(DisplayArea area, double x, double y) =>
        x >= area.Left && x < area.Right && y >= area.Top && y < area.Bottom;

    private static double DistanceSquared(DisplayArea area, double x, double y)
    {
        var nearestX = Math.Clamp(x, area.Left, area.Right);
        var nearestY = Math.Clamp(y, area.Top, area.Bottom);
        var deltaX = x - nearestX;
        var deltaY = y - nearestY;
        return (deltaX * deltaX) + (deltaY * deltaY);
    }

    private sealed record ScreenArea(System.Windows.Forms.Screen Screen, DisplayArea Area);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct NativePoint
    {
        public NativePoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }

        public int Y { get; }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(NativePoint point, uint flags);

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr monitor, int dpiType, out uint dpiX, out uint dpiY);

    [DllImport("user32.dll")]
    private static extern uint GetDpiForSystem();
}
