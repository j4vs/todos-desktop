using System.Windows;
using System.Windows.Media;
using Todos.Domain.Models;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace Todos.Presentation.Themes;

public static class WindowThemeResources
{
    public static void Apply(ResourceDictionary resources, TodoColorTheme theme)
    {
        var palette = TodoThemePalette.Get(theme);
        var accent = Parse(palette.Accent);
        var surface = Parse(palette.Surface);
        var text = Parse(palette.Text);
        Set("TodosBrandBrush", accent);
        Set("TodosBrandHoverBrush", Mix(accent, text, 0.12));
        Set("TodosBrandPressedBrush", Mix(accent, text, 0.25));
        Set("TodosFocusBrush", accent);
        Set("TodosTextBrush", text);
        Set("TodosSecondaryTextBrush", Mix(text, surface, 0.3));
        Set("TodosPopupBrush", surface);
        Set("TodosHoverSurfaceBrush", Mix(surface, accent, 0.14));
        Set("TodosPressedSurfaceBrush", Mix(surface, accent, 0.26));
        Set("WindowInputBorderBrush", Mix(surface, accent, 0.4));

        void Set(string key, Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            resources[key] = brush;
        }
    }

    private static Color Parse(string value) => (Color)ColorConverter.ConvertFromString(value);
    private static Color Mix(Color from, Color to, double amount) => Color.FromRgb(
        (byte)Math.Round(from.R + (to.R - from.R) * amount),
        (byte)Math.Round(from.G + (to.G - from.G) * amount),
        (byte)Math.Round(from.B + (to.B - from.B) * amount));
}
