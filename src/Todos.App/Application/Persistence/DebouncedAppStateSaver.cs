using Todos.Domain;
using Todos.Domain.Models;

namespace Todos.Application.Persistence;

public sealed class DebouncedAppStateSaver : IAsyncDisposable
{
    private readonly IAppStateRepository _repository;
    private readonly object _sync = new();
    private readonly SemaphoreSlim _saveGate = new(1, 1);
    private CancellationTokenSource? _delayCancellation;
    private AppState? _pendingSnapshot;
    private Task _backgroundSave = Task.CompletedTask;
    private bool _disposed;
    public event Action<AppState, Exception?>? SaveCompleted;

    public DebouncedAppStateSaver(IAppStateRepository repository)
    {
        _repository = repository;
    }

    public void Schedule(AppState state, TimeSpan? delay = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var snapshot = AppStateSnapshot.Clone(state);
        CancellationTokenSource cancellation;

        lock (_sync)
        {
            _pendingSnapshot = snapshot;
            _delayCancellation?.Cancel();
            _delayCancellation?.Dispose();
            _delayCancellation = new CancellationTokenSource();
            cancellation = _delayCancellation;
            _backgroundSave = SaveAfterDelayAsync(delay ?? AppConstants.DefaultSaveDebounce, cancellation.Token);
        }
    }

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        AppState? snapshot;
        lock (_sync)
        {
            _delayCancellation?.Cancel();
            snapshot = _pendingSnapshot;
            _pendingSnapshot = null;
        }

        if (snapshot is not null)
        {
            await SaveSnapshotAsync(snapshot, cancellationToken);
        }
        else
        {
            await WaitForBackgroundSaveAsync(cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await FlushAsync();
        _disposed = true;
        _delayCancellation?.Dispose();
        _saveGate.Dispose();
    }

    private async Task SaveAfterDelayAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(delay, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        AppState? snapshot;
        lock (_sync)
        {
            snapshot = _pendingSnapshot;
            _pendingSnapshot = null;
        }

        if (snapshot is not null)
        {
            await SaveSnapshotAsync(snapshot, CancellationToken.None);
        }
    }

    private async Task SaveSnapshotAsync(AppState snapshot, CancellationToken cancellationToken)
    {
        await _saveGate.WaitAsync(cancellationToken);
        try
        {
            await _repository.SaveAsync(snapshot, cancellationToken);
            SaveCompleted?.Invoke(snapshot, null);
        }
        catch (Exception exception)
        {
            SaveCompleted?.Invoke(snapshot, exception);
            throw;
        }
        finally
        {
            _saveGate.Release();
        }
    }

    private async Task WaitForBackgroundSaveAsync(CancellationToken cancellationToken)
    {
        Task background;
        lock (_sync)
        {
            background = _backgroundSave;
        }

        await background.WaitAsync(cancellationToken);
    }
}
