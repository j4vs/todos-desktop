using Todos.Application;
using Todos.Application.Persistence;
using Todos.Infrastructure.Logging;
using Todos.Infrastructure.Persistence;
using Todos.Infrastructure.Windows;
using Todos.Presentation;

namespace Todos.App;

public partial class App : System.Windows.Application
{
    private AppController? _controller;
    private SingleInstanceService? _singleInstance;
    private LocalLogService? _log;

    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var launchOptions = AppLaunchOptions.Parse(e.Args);
            var paths = new AppDataPaths(launchOptions.DataRoot);
            _log = new LocalLogService(paths);
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            _singleInstance = new SingleInstanceService(
                launchOptions.InstanceName,
                () => Dispatcher.BeginInvoke(() => _controller?.ShowFromExternalActivation()));
            if (!_singleInstance.TryBecomePrimary())
            {
                _singleInstance.SignalPrimary();
                Shutdown();
                return;
            }

            var repository = new JsonAppStateRepository(paths);
            var loadResult = await repository.LoadAsync();
            var saver = new DebouncedAppStateSaver(repository);
            var startupRegistration = new StartupRegistrationService(new WindowsStartupRegistry());

            _controller = new AppController(
                loadResult.State,
                saver,
                loadResult.Warning,
                launchOptions,
                startupRegistration,
                _log);
            await _controller.StartAsync();
        }
        catch (Exception exception)
        {
            _log?.Error("Application startup failed.", exception);
            MessageBox.Show(
                $"Todo's no pudo iniciar.\n\n{exception.Message}",
                "Todo's",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        _controller?.Dispose();
        _singleInstance?.Dispose();
        DispatcherUnhandledException -= OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        base.OnExit(e);
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        _log?.Error("Unhandled UI error.", e.Exception);
        MessageBox.Show(
            "Todo's encontró un error inesperado, pero intentará continuar. Los cambios guardados permanecen seguros.",
            "Error inesperado",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
        e.Handled = true;
    }

    private void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            _log?.Error("Unhandled background error.", exception);
        }
    }
}
