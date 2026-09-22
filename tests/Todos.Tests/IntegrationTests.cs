using System.Diagnostics;
using Todos.Application.Persistence;
using Todos.Domain;
using Todos.Domain.Models;
using Todos.Infrastructure.Persistence;

namespace Todos.Tests;

internal static class IntegrationTests
{
    [Test]
    public static async Task CorruptPrimaryAndBackupStartEmptyWithoutDeletingEvidence()
    {
        using var directory = new TemporaryDirectory();
        var paths = new AppDataPaths(directory.Path);
        await File.WriteAllTextAsync(paths.StateFile, "{ primary corrupt");
        await File.WriteAllTextAsync(paths.BackupFile, "{ backup corrupt");
        var repository = new JsonAppStateRepository(paths);

        var result = await repository.LoadAsync();

        Assert.Equal(AppStateLoadSource.Empty, result.Source);
        Assert.Equal(0, result.State.TodoWindows.Count);
        Assert.NotNull(result.Warning);
        Assert.True(File.Exists(paths.StateFile));
        Assert.True(File.Exists(paths.BackupFile));
    }

    [Test]
    public static async Task TwentyListsAndOneThousandTasksRoundTripWithinAReasonableTime()
    {
        using var directory = new TemporaryDirectory();
        var repository = new JsonAppStateRepository(new AppDataPaths(directory.Path));
        var state = CreateLargeState();
        var timer = Stopwatch.StartNew();

        await repository.SaveAsync(state);
        var loaded = await repository.LoadAsync();
        timer.Stop();

        Assert.Equal(20, loaded.State.TodoWindows.Count);
        Assert.Equal(1000, loaded.State.TodoWindows.Sum(window => window.Tasks.Count));
        Assert.Equal(10, loaded.State.TodoWindows.Count(window => window.IsVisible));
        Assert.Equal(AppConstants.DefaultWindowWidth, loaded.State.TodoWindows[7].Width);
        Assert.Equal("Lista 20 — revisión ñ", loaded.State.TodoWindows[^1].Title);
        Assert.True(timer.Elapsed < TimeSpan.FromSeconds(5), $"Large state round-trip took {timer.Elapsed.TotalSeconds:N2} seconds.");
    }

    [Test]
    public static void ADeepSnapshotIsUnaffectedByLaterUiMutations()
    {
        var state = CreateLargeState();
        var snapshot = AppStateSnapshot.Clone(state);

        state.TodoWindows[0].Title = "Mutada";
        state.TodoWindows[0].Tasks[0].Text = "Contenido mutado";
        state.TodoWindows.RemoveAt(1);

        Assert.Equal(20, snapshot.TodoWindows.Count);
        Assert.Equal("Lista 1 — revisión ñ", snapshot.TodoWindows[0].Title);
        Assert.Equal("Paso 1 de la lista 1", snapshot.TodoWindows[0].Tasks[0].Text);
    }

    private static AppState CreateLargeState()
    {
        var now = DateTimeOffset.UtcNow;
        var state = new AppState();
        for (var windowIndex = 0; windowIndex < 20; windowIndex++)
        {
            var window = new TodoWindowState
            {
                Title = $"Lista {windowIndex + 1} — revisión ñ",
                ColorTheme = (TodoColorTheme)(windowIndex % Enum.GetValues<TodoColorTheme>().Length),
                PositionX = -1200 + (windowIndex * 45),
                PositionY = 30 + (windowIndex * 18),
                IsVisible = windowIndex % 2 == 0,
                AreCompletedTasksExpanded = windowIndex % 3 == 0,
            };

            for (var taskIndex = 0; taskIndex < 50; taskIndex++)
            {
                var task = new TodoTaskItem
                {
                    Text = $"Paso {taskIndex + 1} de la lista {windowIndex + 1}",
                    IsCompleted = taskIndex % 4 == 0,
                    PendingOrder = taskIndex,
                    CreatedAt = now.AddMinutes(taskIndex),
                    UpdatedAt = now.AddMinutes(taskIndex),
                };
                if (task.IsCompleted)
                {
                    task.CompletedAt = now.AddMinutes(taskIndex + 1);
                    task.PendingOrder = -1;
                }

                window.Tasks.Add(task);
            }

            state.TodoWindows.Add(window);
        }

        AppStateNormalizer.Normalize(state);
        return state;
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        private readonly string _temporaryRoot = System.IO.Path.GetFullPath(System.IO.Path.GetTempPath());

        public TemporaryDirectory()
        {
            Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(_temporaryRoot, $"todos-integration-{Guid.NewGuid():N}"));
            if (!Path.StartsWith(_temporaryRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The test directory escaped the temporary root.");
            }

            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            var resolved = System.IO.Path.GetFullPath(Path);
            if (resolved.StartsWith(_temporaryRoot, StringComparison.OrdinalIgnoreCase) && Directory.Exists(resolved))
            {
                Directory.Delete(resolved, recursive: true);
            }
        }
    }
}
