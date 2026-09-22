using Todos.Application.Windows;

namespace Todos.Infrastructure.Windows;

public sealed class StartupRegistrationService : IStartupRegistrationService
{
    public const string DefaultValueName = "Todos";
    private readonly IStartupRegistry _registry;
    private readonly Func<string> _executablePath;
    private readonly string _valueName;

    public StartupRegistrationService(
        IStartupRegistry registry,
        Func<string>? executablePath = null,
        string valueName = DefaultValueName)
    {
        _registry = registry;
        _executablePath = executablePath ?? GetCurrentExecutablePath;
        _valueName = valueName;
    }

    public StartupRegistrationStatus GetStatus()
    {
        var registeredCommand = _registry.GetValue(_valueName);
        if (string.IsNullOrWhiteSpace(registeredCommand))
        {
            return new StartupRegistrationStatus(false, false, null);
        }

        return new StartupRegistrationStatus(
            true,
            string.Equals(registeredCommand, BuildCommand(_executablePath()), StringComparison.OrdinalIgnoreCase),
            registeredCommand);
    }

    public void Enable() => _registry.SetValue(_valueName, BuildCommand(_executablePath()));

    public void Disable() => _registry.DeleteValue(_valueName);

    public static string BuildCommand(string executablePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        return $"\"{Path.GetFullPath(executablePath)}\" --autostart";
    }

    private static string GetCurrentExecutablePath() =>
        Environment.ProcessPath ?? throw new InvalidOperationException("No se pudo determinar la ruta de Todo's.");
}
