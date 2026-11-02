using Todos.Domain.Models;

namespace Todos.Presentation.Themes;

public static class TodoThemePalette
{
    private static readonly IReadOnlyDictionary<TodoColorTheme, TodoThemeColors> Colors =
        new Dictionary<TodoColorTheme, TodoThemeColors>
        {
            [TodoColorTheme.Butter] = new("#FFF2B8", "#FFF9DE", "#D89A30", "#332F2A"),
            [TodoColorTheme.Rose] = new("#FFDCE5", "#FFF2F5", "#C96C87", "#352C31"),
            [TodoColorTheme.Lavender] = new("#E7DDF8", "#F6F1FF", "#7B6FC2", "#302D3A"),
            [TodoColorTheme.Sky] = new("#DCEEFF", "#F0F8FF", "#568FC2", "#293440"),
            [TodoColorTheme.Mint] = new("#DDF5E8", "#F0FBF5", "#4E9B76", "#29352F"),
            [TodoColorTheme.Peach] = new("#FFE2CF", "#FFF3EA", "#C98257", "#392F29"),
            [TodoColorTheme.Mist] = new("#EAE8E5", "#F7F6F4", "#77716B", "#302E2C"),
        };

    // Use the theme accent with a small brightness reduction for clear white icons.
    public static string GetLauncherBackground(TodoColorTheme theme)
    {
        var background = Get(theme).Accent;
        var channels = Enumerable.Range(0, 3)
            .Select(index => (int)Math.Round(Convert.ToInt32(background.Substring(1 + index * 2, 2), 16) * 0.9));
        return "#" + string.Concat(channels.Select(channel => channel.ToString("X2")));
    }

    public static TodoThemeColors Get(TodoColorTheme theme) =>
        Colors.TryGetValue(theme, out var colors) ? colors : Colors[TodoColorTheme.Lavender];
}

public sealed record TodoThemeColors(string Background, string Surface, string Accent, string Text);
