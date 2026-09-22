namespace Todos.Application.Windows;

public static class WindowPositioning
{
    public static (double X, double Y) EnsureVisible(
        double? x,
        double? y,
        double width,
        double height,
        DisplayArea area,
        double margin = 8)
    {
        var defaultX = area.Right - width - 24;
        var defaultY = area.Top + Math.Max(24, (area.Height - height) / 2);
        var candidateX = IsFinite(x) ? x!.Value : defaultX;
        var candidateY = IsFinite(y) ? y!.Value : defaultY;
        var minimumX = area.Left + margin;
        var minimumY = area.Top + margin;
        var maximumX = Math.Max(minimumX, area.Right - width - margin);
        var maximumY = Math.Max(minimumY, area.Bottom - height - margin);
        return (Math.Clamp(candidateX, minimumX, maximumX), Math.Clamp(candidateY, minimumY, maximumY));
    }

    private static bool IsFinite(double? value) => value.HasValue && double.IsFinite(value.Value);
}
