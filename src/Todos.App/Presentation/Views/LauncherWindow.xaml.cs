using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Todos.Presentation.ViewModels;
using Todos.Presentation.Windows;

namespace Todos.Presentation.Views;

public partial class LauncherWindow : System.Windows.Window
{
    private static readonly nint HwndTopmost = new(-1);
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoActivate = 0x0010;
    private System.Windows.Point _mouseDownScreenPosition;
    private double _windowDownLeft;
    private double _windowDownTop;
    private bool _isPointerCaptured;
    private bool _wasDragged;

    public LauncherWindow(LauncherViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        MenuPopup.CustomPopupPlacementCallback = PlacePopup;
        Closing += OnClosing;
        Loaded += (_, _) => LauncherSurface.Focus();
    }

    public event Action<double, double>? PositionCommitted;

    public bool AllowClose { get; set; }

    public FrameworkElement MenuPreviewContent =>
        MenuPopup.Child as FrameworkElement
        ?? throw new InvalidOperationException("El menú no tiene contenido visual.");

    public FrameworkElement HiddenSubmenuPreviewContent =>
        HiddenListsPopup.Child as FrameworkElement
        ?? throw new InvalidOperationException("El submenú no tiene contenido visual.");

    public FrameworkElement LauncherButtonPreviewContent => LauncherSurface;

    public void BringLauncherToFront()
    {
        if (!IsVisible)
        {
            Show();
        }

        Topmost = false;
        Topmost = true;
        Activate();
    }

    public void KeepAboveTodoWindows()
    {
        if (!IsLoaded)
        {
            return;
        }

        var handle = new WindowInteropHelper(this).Handle;
        if (handle != nint.Zero)
        {
            SetWindowPos(handle, HwndTopmost, 0, 0, 0, 0, SwpNoMove | SwpNoSize | SwpNoActivate);
        }
    }

    private void OnLauncherMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left ||
            !LauncherInputPolicy.IsInside(LauncherSurface, e.OriginalSource as DependencyObject))
        {
            return;
        }

        _mouseDownScreenPosition = GetCursorPositionInDips();
        _windowDownLeft = Left;
        _windowDownTop = Top;
        _wasDragged = false;
        _isPointerCaptured = LauncherSurface.CaptureMouse();
        e.Handled = true;
    }


    private void OnLauncherMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!_isPointerCaptured || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var current = GetCursorPositionInDips();
        var deltaX = current.X - _mouseDownScreenPosition.X;
        var deltaY = current.Y - _mouseDownScreenPosition.Y;
        if (!_wasDragged &&
            Math.Abs(deltaX) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(deltaY) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        _wasDragged = true;
        Left = _windowDownLeft + deltaX;
        Top = _windowDownTop + deltaY;
        e.Handled = true;
    }

    private void OnLauncherMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isPointerCaptured || e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        LauncherSurface.ReleaseMouseCapture();
        _isPointerCaptured = false;
        if (_wasDragged)
        {
            PositionCommitted?.Invoke(Left, Top);
        }
        else if (DataContext is LauncherViewModel viewModel)
        {
            viewModel.ToggleMenu();
        }

        e.Handled = true;
    }

    private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not LauncherViewModel viewModel)
        {
            return;
        }

        if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
        {
            viewModel.CreateListCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            viewModel.IsMenuOpen = false;
            LauncherSurface.Focus();
            e.Handled = true;
        }
        else if ((e.Key == Key.Enter || e.Key == Key.Space) &&
                 ReferenceEquals(Keyboard.FocusedElement, LauncherSurface))
        {
            viewModel.ToggleMenu();
            e.Handled = true;
        }
    }

    private System.Windows.Point GetCursorPositionInDips()
    {
        var cursor = System.Windows.Forms.Cursor.Position;
        var dpi = VisualTreeHelper.GetDpi(this);
        return new System.Windows.Point(cursor.X / dpi.DpiScaleX, cursor.Y / dpi.DpiScaleY);
    }

    private static CustomPopupPlacement[] PlacePopup(
        System.Windows.Size popupSize,
        System.Windows.Size targetSize,
        System.Windows.Point offset)
    {
        const double gap = 12;
        var centeredY = (targetSize.Height - popupSize.Height) / 2;
        return
        [
            new CustomPopupPlacement(new System.Windows.Point(targetSize.Width + gap, centeredY), PopupPrimaryAxis.Vertical),
            new CustomPopupPlacement(new System.Windows.Point(-popupSize.Width - gap, centeredY), PopupPrimaryAxis.Vertical),
        ];
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (!AllowClose)
        {
            e.Cancel = true;
            Hide();
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(
        nint hWnd,
        nint hWndInsertAfter,
        int x,
        int y,
        int width,
        int height,
        uint flags);
}
