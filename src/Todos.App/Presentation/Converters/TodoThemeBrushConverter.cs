using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Todos.Domain.Models;
using Todos.Presentation.Themes;

namespace Todos.Presentation.Converters;

public sealed class TodoThemeBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var theme = value is TodoColorTheme colorTheme ? colorTheme : TodoColorTheme.Lavender;
        var colors = TodoThemePalette.Get(theme);
        var key = parameter as string;
        var color = key switch
        {
            "Launcher" => TodoThemePalette.GetLauncherBackground(theme),
            "Surface" => colors.Surface,
            "Accent" => colors.Accent,
            "Text" => colors.Text,
            _ => colors.Background,
        };

        return (SolidColorBrush)new BrushConverter().ConvertFromString(color)!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
