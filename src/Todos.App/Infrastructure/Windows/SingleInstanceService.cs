namespace Todos.Infrastructure.Windows;

public sealed class SingleInstanceService : IDisposable
{
    private readonly string _mutexName;
    private readonly string _activationEventName;
    private readonly Action _activationRequested;
    private Mutex? _mutex;
    private EventWaitHandle? _activationEvent;
    private CancellationTokenSource? _cancellation;
    private Task? _listener;
    private bool _ownsMutex;

    public SingleInstanceService(string applicationId, Action activationRequested)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
        _mutexName = $"Local\\{applicationId}.SingleInstance";
        _activationEventName = $"Local\\{applicationId}.Activate";
        _activationRequested = activationRequested;
    }

    public bool TryBecomePrimary()
    {
        _mutex = new Mutex(initiallyOwned: true, _mutexName, out var createdNew);
        _ownsMutex = createdNew;
        if (!createdNew)
        {
            _mutex.Dispose();
            _mutex = null;
            return false;
        }

        _activationEvent = new EventWaitHandle(false, EventResetMode.AutoReset, _activationEventName);
        _cancellation = new CancellationTokenSource();
        _listener = ListenForActivationAsync(_cancellation.Token);
        return true;
    }

    public void SignalPrimary()
    {
        try
        {
            using var activationEvent = EventWaitHandle.OpenExisting(_activationEventName);
            activationEvent.Set();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
            // The primary instance is already closing.
        }
    }

    public void Dispose()
    {
        _cancellation?.Cancel();
        _activationEvent?.Set();
        try
        {
            _listener?.Wait(TimeSpan.FromSeconds(1));
        }
        catch (AggregateException)
        {
            // Cancellation is expected during shutdown.
        }

        _cancellation?.Dispose();
        _activationEvent?.Dispose();
        if (_ownsMutex)
        {
            _mutex?.ReleaseMutex();
        }

        _mutex?.Dispose();
    }

    private async Task ListenForActivationAsync(CancellationToken cancellationToken)
    {
        var activationEvent = _activationEvent
            ?? throw new InvalidOperationException("The activation event has not been initialized.");

        await Task.Run(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                activationEvent.WaitOne();
                if (!cancellationToken.IsCancellationRequested)
                {
                    _activationRequested();
                }
            }
        }, cancellationToken);
    }
}
