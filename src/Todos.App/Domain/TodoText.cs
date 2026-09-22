namespace Todos.Domain;

public static class TodoText
{
    public static string NormalizeTitle(string? value)
    {
        var normalized = NormalizeSingleLogicalLine(value);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return "Nueva lista";
        }

        return Truncate(normalized, AppConstants.MaxTitleLength);
    }

    public static string NormalizeTask(string? value)
    {
        var normalized = NormalizeSingleLogicalLine(value);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("La tarea no puede estar vacía.", nameof(value));
        }

        return Truncate(normalized, AppConstants.MaxTaskTextLength);
    }

    private static string NormalizeSingleLogicalLine(string? value) =>
        string.Join(' ', (value ?? string.Empty)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        .Trim();

    private static string Truncate(string value, int maximumLength) =>
        value.Length <= maximumLength ? value : value[..maximumLength];
}
