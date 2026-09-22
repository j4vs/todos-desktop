using Todos.Infrastructure.Logging;
using Todos.Infrastructure.Persistence;
using Todos.Infrastructure.Windows;

namespace Todos.Tests;

internal static class LifecycleTests
{
    [Test]
    public static void StartupRegistrationUsesQuotedExecutableAndCanBeDisabled()
    {
        var registry = new MemoryStartupRegistry();
        var executable = Path.Combine(Path.GetTempPath(), "Todo's portable", "Todos.exe");
        var service = new StartupRegistrationService(registry, () => executable);

        Assert.False(service.GetStatus().IsRegistered);
        service.Enable();

        var status = service.GetStatus();
        Assert.True(status.IsRegistered);
        Assert.True(status.MatchesCurrentExecutable);
        Assert.Equal($"\"{Path.GetFullPath(executable)}\" --autostart", status.RegisteredCommand);

        service.Disable();
        Assert.False(service.GetStatus().IsRegistered);
    }

    [Test]
    public static void StartupRegistrationDetectsAnExecutableThatMoved()
    {
        var registry = new MemoryStartupRegistry();
        registry.SetValue(StartupRegistrationService.DefaultValueName, "\"C:\\Old\\Todos.exe\" --autostart");
        var service = new StartupRegistrationService(registry, () => "C:\\New\\Todos.exe");

        var status = service.GetStatus();

        Assert.True(status.IsRegistered);
        Assert.False(status.MatchesCurrentExecutable);
    }

    [Test]
    public static void SecondInstanceSignalsThePrimaryInstance()
    {
        var applicationId = $"Todos.Tests.{Guid.NewGuid():N}";
        using var activated = new ManualResetEventSlim();
        using var primary = new SingleInstanceService(applicationId, activated.Set);
        using var secondary = new SingleInstanceService(applicationId, () => { });

        Assert.True(primary.TryBecomePrimary());
        Assert.False(secondary.TryBecomePrimary());
        secondary.SignalPrimary();

        Assert.True(activated.Wait(TimeSpan.FromSeconds(2)), "The primary instance did not receive activation.");
    }

    [Test]
    public static void LocalLogCreatesAnOperationalLogWithoutThrowing()
    {
        var root = Path.Combine(Path.GetTempPath(), $"todos-log-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var paths = new AppDataPaths(root);
            var log = new LocalLogService(paths);

            log.Information("Application lifecycle started.");
            log.Error("A controlled operation failed.", new InvalidOperationException("controlled"));

            var content = File.ReadAllText(paths.LogFile);
            Assert.True(content.Contains("[INFO] Application lifecycle started.", StringComparison.Ordinal));
            Assert.True(content.Contains("[ERROR] A controlled operation failed.", StringComparison.Ordinal));
            Assert.True(content.Contains("InvalidOperationException: controlled", StringComparison.Ordinal));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private sealed class MemoryStartupRegistry : IStartupRegistry
    {
        private readonly Dictionary<string, string> _values = [];

        public string? GetValue(string name) => _values.GetValueOrDefault(name);

        public void SetValue(string name, string value) => _values[name] = value;

        public void DeleteValue(string name) => _values.Remove(name);
    }
}
