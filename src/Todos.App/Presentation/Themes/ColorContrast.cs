using System.Globalization;

namespace Todos.Presentation.Themes;

public static class ColorContrast
{
    public static double Ratio(string firstHex, string secondHex)
    {
        var first = RelativeLuminance(Parse(firstHex));
        var second = RelativeLuminance(Parse(secondHex));
        var lighter = Math.Max(first, second);
        var darker = Math.Min(first, second);
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static (byte Red, byte Green, byte Blue) Parse(string hex)
    {
        var value = hex.TrimStart('#');
        if (value.Length == 8)
        {
            value = value[2..];
        }

        if (value.Length != 6)
        {
            throw new FormatException($"El color '{hex}' no usa formato RGB hexadecimal.");
        }

        return (
            byte.Parse(value[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture),
            byte.Parse(value.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
            byte.Parse(value.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
    }

    private static double RelativeLuminance((byte Red, byte Green, byte Blue) color) =>
        (0.2126 * Linearize(color.Red)) +
        (0.7152 * Linearize(color.Green)) +
        (0.0722 * Linearize(color.Blue));

    private static double Linearize(byte channel)
    {
        var value = channel / 255d;
        return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
    }
}
