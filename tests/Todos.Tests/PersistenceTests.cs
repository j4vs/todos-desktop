using Todos.Application.Persistence;
using Todos.Domain;
using Todos.Domain.Models;
using Todos.Infrastructure.Persistence;

namespace Todos.Tests;

internal static class PersistenceTests
{
    [Test]
    public static async Task QuickAccessSurvivesSnapshotSaveRestartAndDeletedAssociationIsCleared()
    {
        using var directory = new TemporaryDirectory();
        var repository = new JsonAppStateRepository(new AppDataPaths(directory.Path));
        var state = CreateState("Conservar tarea existente");
        state.Settings.AutoMinimizeWindows = true;
        state.LauncherState.LastMinimizedWindowId = state.TodoWindows[0].Id;
        var snapshot = AppStateSnapshot.Clone(state);
        state.Settings.AutoMinimizeWindows = false;
        state.LauncherState.LastMinimizedWindowId = null;
        await repository.SaveAsync(snapshot);
        var restored = (await repository.LoadAsync()).State;
        Assert.True(restored.Settings.AutoMinimizeWindows);
        Assert.Equal(snapshot.TodoWindows[0].Id, restored.LauncherState.LastMinimizedWindowId);
        Assert.Equal("Conservar tarea existente", restored.TodoWindows[0].Tasks[0].Text);
        restored.TodoWindows.Clear();
        restored.TodoWindows.Add(new TodoWindowState { IsVisible = false });
        Assert.Equal<Guid?>(null, AppStateNormalizer.Normalize(restored).LauncherState.LastMinimizedWindowId);
    }

    [Test]
    public static async Task StateRoundTripPreservesUnicodeAndSettings()
    {
        using var directory = new TemporaryDirectory();
        var repository = new JsonAppStateRepository(new AppDataPaths(directory.Path));
        var state = CreateState("Revisar razón social y código postal ñ");
        state.Settings.RunAtWindowsStartup = false;

        await repository.SaveAsync(state);
        var result = await repository.LoadAsync();

        Assert.Equal(AppStateLoadSource.Primary, result.Source);
        Assert.Equal("Revisar razón social y código postal ñ", result.State.TodoWindows[0].Tasks[0].Text);
        Assert.False(result.State.Settings.RunAtWindowsStartup);
        Assert.True(result.State.LastSavedAt != default);
    }

    [Test]
    public static async Task CorruptPrimaryFallsBackToPreviousBackup()
    {
        using var directory = new TemporaryDirectory();
        var paths = new AppDataPaths(directory.Path);
        var repository = new JsonAppStateRepository(paths);
        await repository.SaveAsync(CreateState("Versión uno"));
        await repository.SaveAsync(CreateState("Versión dos"));
        await File.WriteAllTextAsync(paths.StateFile, "{ archivo dañado");

        var result = await repository.LoadAsync();

        Assert.Equal(AppStateLoadSource.Backup, result.Source);
        Assert.Equal("Versión uno", result.State.TodoWindows[0].Tasks[0].Text);
        Assert.NotNull(result.Warning);
    }

    [Test]
    public static async Task MissingOptionalFieldsUseSafeDefaults()
    {
        using var directory = new TemporaryDirectory();
        var paths = new AppDataPaths(directory.Path);
        Directory.CreateDirectory(directory.Path);
        await File.WriteAllTextAsync(paths.StateFile, "{\"schemaVersion\":1,\"unexpected\":true,\"todoWindows\":[]}");
        var repository = new JsonAppStateRepository(paths);

        var result = await repository.LoadAsync();

        Assert.Equal(AppStateLoadSource.Primary, result.Source);
        Assert.NotNull(result.State.Settings);
        Assert.True(result.State.Settings.RunAtWindowsStartup);
        Assert.NotNull(result.State.LauncherState);
        Assert.False(result.State.Settings.AutoMinimizeWindows);
        Assert.Equal<Guid?>(null, result.State.LauncherState.LastMinimizedWindowId);
    }

    [Test]
    public static async Task FailedTemporaryWritePreservesPrimary()
    {
        using var directory = new TemporaryDirectory();
        var paths = new AppDataPaths(directory.Path);
        var repository = new JsonAppStateRepository(paths);
        await repository.SaveAsync(CreateState("Estado válido"));

        await using (var locked = new FileStream(paths.TemporaryFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
        {
            await Assert.ThrowsAsync<IOException>(() => repository.SaveAsync(CreateState("No debe reemplazar")));
        }

        var result = await repository.LoadAsync();
        Assert.Equal("Estado válido", result.State.TodoWindows[0].Tasks[0].Text);
    }

    [Test]
    public static async Task DebouncedSaverPersistsOnlyLatestPendingSnapshot()
    {
        var repository = new RecordingRepository();
        await using var saver = new DebouncedAppStateSaver(repository);
        saver.Schedule(CreateState("Primero"), TimeSpan.FromSeconds(30));
        saver.Schedule(CreateState("Último"), TimeSpan.FromSeconds(30));

        await saver.FlushAsync();

        Assert.Equal(1, repository.SavedStates.Count);
        Assert.Equal("Último", repository.SavedStates[0].TodoWindows[0].Tasks[0].Text);
    }

    private static AppState CreateState(string taskText)
    {
        var state = new AppState();
        var window = new TodoWindowState { Title = "Proyecto" };
        TodoListOperations.AddTask(window, taskText, DateTimeOffset.UtcNow);
        state.TodoWindows.Add(window);
        return state;
    }

    private sealed class RecordingRepository : IAppStateRepository
    {
        public List<AppState> SavedStates { get; } = [];

        public Task<AppStateLoadResult> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new AppStateLoadResult(new AppState(), AppStateLoadSource.Empty));

        public Task SaveAsync(AppState state, CancellationToken cancellationToken = default)
        {
            SavedStates.Add(AppStateSnapshot.Clone(state));
            return Task.CompletedTask;
        }
    }

    internal sealed class TemporaryDirectory : IDisposable
    {
        private readonly string _temporaryRoot = System.IO.Path.GetFullPath(System.IO.Path.GetTempPath());

        public TemporaryDirectory()
        {
            Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(_temporaryRoot, $"todos-tests-{Guid.NewGuid():N}"));
            if (!Path.StartsWith(_temporaryRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("La carpeta de prueba debe permanecer dentro del directorio temporal.");
            }

            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            var resolvedPath = System.IO.Path.GetFullPath(Path);
            if (resolvedPath.StartsWith(_temporaryRoot, StringComparison.OrdinalIgnoreCase) && Directory.Exists(resolvedPath))
            {
                Directory.Delete(resolvedPath, recursive: true);
            }
        }
    }
}
