namespace Todos.Infrastructure.Persistence;

public sealed class AppDataPaths
{
    public AppDataPaths(string? rootDirectory = null)
    {
        RootDirectory = rootDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Todos");
    }

    public string RootDirectory { get; }

    public string StateFile => Path.Combine(RootDirectory, "state.json");

    public string BackupFile => Path.Combine(RootDirectory, "state.backup.json");

    public string TemporaryFile => Path.Combine(RootDirectory, "state.tmp.json");

    public string LogFile => Path.Combine(RootDirectory, "todos.log");
}
