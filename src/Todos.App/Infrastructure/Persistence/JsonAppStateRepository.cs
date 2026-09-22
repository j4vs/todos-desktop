using System.Text.Json;
using System.Text.Json.Serialization;
using Todos.Application.Persistence;
using Todos.Domain;
using Todos.Domain.Models;

namespace Todos.Infrastructure.Persistence;

public sealed class JsonAppStateRepository : IAppStateRepository
{
    private readonly AppDataPaths _paths;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _writeGate = new(1, 1);

    public JsonAppStateRepository(AppDataPaths paths)
    {
        _paths = paths;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };
        _jsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public async Task<AppStateLoadResult> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_paths.StateFile))
        {
            if (!File.Exists(_paths.BackupFile))
            {
                return new AppStateLoadResult(new AppState(), AppStateLoadSource.Empty);
            }

            return await LoadBackupAsync("No se encontró el archivo principal; se recuperó el respaldo.", cancellationToken);
        }

        try
        {
            var state = await ReadStateAsync(_paths.StateFile, cancellationToken);
            return new AppStateLoadResult(state, AppStateLoadSource.Primary);
        }
        catch (UnsupportedSchemaVersionException)
        {
            throw;
        }
        catch (Exception primaryException) when (IsRecoverableLoadError(primaryException))
        {
            if (!File.Exists(_paths.BackupFile))
            {
                return new AppStateLoadResult(
                    new AppState(),
                    AppStateLoadSource.Empty,
                    "No fue posible leer los datos guardados. El archivo dañado se conservó para recuperación.");
            }

            return await LoadBackupAsync(
                "El archivo principal estaba dañado; Todo's recuperó el respaldo más reciente.",
                cancellationToken);
        }
    }

    public async Task SaveAsync(AppState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        var snapshot = AppStateSnapshot.Clone(state);
        snapshot.LastSavedAt = DateTimeOffset.UtcNow;
        AppStateNormalizer.Normalize(snapshot);

        await _writeGate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(_paths.RootDirectory);
            var json = JsonSerializer.Serialize(snapshot, _jsonOptions);
            await WriteTemporaryFileAsync(json, cancellationToken);
            ReplacePrimaryWithTemporary();
        }
        finally
        {
            _writeGate.Release();
        }
    }

    private async Task<AppStateLoadResult> LoadBackupAsync(string warning, CancellationToken cancellationToken)
    {
        try
        {
            var state = await ReadStateAsync(_paths.BackupFile, cancellationToken);
            return new AppStateLoadResult(state, AppStateLoadSource.Backup, warning);
        }
        catch (UnsupportedSchemaVersionException)
        {
            throw;
        }
        catch (Exception backupException) when (IsRecoverableLoadError(backupException))
        {
            return new AppStateLoadResult(
                new AppState(),
                AppStateLoadSource.Empty,
                "Los archivos de datos no se pudieron leer. Se inició una sesión vacía sin borrar los archivos existentes.");
        }
    }

    private async Task<AppState> ReadStateAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        var state = await JsonSerializer.DeserializeAsync<AppState>(stream, _jsonOptions, cancellationToken);
        return AppStateNormalizer.Normalize(state);
    }

    private async Task WriteTemporaryFileAsync(string json, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            _paths.TemporaryFile,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            4096,
            FileOptions.Asynchronous | FileOptions.WriteThrough);
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(json.AsMemory(), cancellationToken);
        await writer.FlushAsync(cancellationToken);
        stream.Flush(flushToDisk: true);
    }

    private void ReplacePrimaryWithTemporary()
    {
        if (!File.Exists(_paths.StateFile))
        {
            File.Move(_paths.TemporaryFile, _paths.StateFile, overwrite: true);
            File.Copy(_paths.StateFile, _paths.BackupFile, overwrite: true);
            return;
        }

        try
        {
            File.Replace(_paths.TemporaryFile, _paths.StateFile, _paths.BackupFile, ignoreMetadataErrors: true);
        }
        catch (PlatformNotSupportedException)
        {
            FallbackReplace();
        }
        catch (IOException)
        {
            FallbackReplace();
        }
    }

    private void FallbackReplace()
    {
        File.Copy(_paths.StateFile, _paths.BackupFile, overwrite: true);
        File.Move(_paths.TemporaryFile, _paths.StateFile, overwrite: true);
    }

    private static bool IsRecoverableLoadError(Exception exception) =>
        exception is JsonException or IOException or UnauthorizedAccessException;
}
