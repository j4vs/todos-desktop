namespace Todos.Infrastructure.Windows;

public sealed class NotificationAreaService : IDisposable
{
    private readonly System.Windows.Forms.NotifyIcon _notifyIcon;
    private readonly System.Drawing.Icon? _ownedIcon;
    private bool _disposed;
    private System.Drawing.Font? _menuFont;
    private readonly System.Windows.Forms.ToolStripMenuItem _visibilityItem;

    public NotificationAreaService(Action show, Action createList, Action exit, Action? toggleLauncher = null, Func<bool>? isLauncherVisible = null)
    {
        _ownedIcon = Environment.ProcessPath is { } processPath
            ? System.Drawing.Icon.ExtractAssociatedIcon(processPath)
            : null;

        var menu = new System.Windows.Forms.ContextMenuStrip();
        _visibilityItem = new System.Windows.Forms.ToolStripMenuItem("Ocultar botón", null, (_, _) => (toggleLauncher ?? show)());
        menu.Items.Add(_visibilityItem);
        menu.Opening += (_, _) => UpdateLauncherVisibility(isLauncherVisible?.Invoke() ?? true);
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

    public void UpdateAppearance(double fontScale)
    {
        var next = new System.Drawing.Font("Segoe UI", (float)(9 * fontScale));
        _notifyIcon.ContextMenuStrip!.Font = next;
        _menuFont?.Dispose();
        _menuFont = next;
    }

    public void UpdateLauncherVisibility(bool visible) =>
        _visibilityItem.Text = visible ? "Ocultar botón" : "Mostrar botón";

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
        _menuFont?.Dispose();
    }
}
