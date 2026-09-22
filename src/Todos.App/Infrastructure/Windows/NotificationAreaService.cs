namespace Todos.Infrastructure.Windows;

public sealed class NotificationAreaService : IDisposable
{
    private readonly System.Windows.Forms.NotifyIcon _notifyIcon;
    private readonly System.Drawing.Icon? _ownedIcon;
    private bool _disposed;

    public NotificationAreaService(Action show, Action createList, Action exit)
    {
        _ownedIcon = Environment.ProcessPath is { } processPath
            ? System.Drawing.Icon.ExtractAssociatedIcon(processPath)
            : null;

        var menu = new System.Windows.Forms.ContextMenuStrip();
        menu.Items.Add("Mostrar Todo's", null, (_, _) => show());
        menu.Items.Add("Nueva lista", null, (_, _) => createList());
        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menu.Items.Add("Salir", null, (_, _) => exit());

        _notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Text = "Todo's",
            Icon = _ownedIcon ?? System.Drawing.SystemIcons.Application,
            ContextMenuStrip = menu,
            Visible = true,
        };
        _notifyIcon.DoubleClick += (_, _) => show();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _notifyIcon.Visible = false;
        _notifyIcon.ContextMenuStrip?.Dispose();
        _notifyIcon.Dispose();
        _ownedIcon?.Dispose();
    }
}
