using Todos.Infrastructure.Persistence;

namespace Todos.Infrastructure.Logging;

public sealed class LocalLogService
{
    private const long MaximumLogBytes = 512 * 1024;
    private readonly AppDataPaths _paths;
    private readonly object _sync = new();

    public LocalLogService(AppDataPaths paths)
    {
        _paths = paths;
    }

    public void Information(string message) => Write("INFO", message, null);

    public void Error(string message, Exception exception) => Write("ERROR", message, exception);

    private void Write(string level, string message, Exception? exception)
    {
        try
        {
            lock (_sync)
            {
                Directory.CreateDirectory(_paths.RootDirectory);
                RotateIfNeeded();
                var safeMessage = message.ReplaceLineEndings(" ");
                var exceptionText = exception is null
                    ? string.Empty
                    : $" | {exception.GetType().Name}: {exception.Message.ReplaceLineEndings(" ")}";
                File.AppendAllText(
                    _paths.LogFile,
                    $"{DateTimeOffset.Now:O} [{level}] {safeMessage}{exceptionText}{Environment.NewLine}");
            }
        }
        catch
        {
            // Logging must never prevent the application from continuing or closing.
        }
    }

    private void RotateIfNeeded()
    {
        if (!File.Exists(_paths.LogFile) || new FileInfo(_paths.LogFile).Length < MaximumLogBytes)
        {
            return;
        }

        var previousLog = Path.Combine(_paths.RootDirectory, "todos.previous.log");
        File.Move(_paths.LogFile, previousLog, overwrite: true);
    }
}
