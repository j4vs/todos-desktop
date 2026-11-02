using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Media.Animation;
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
    // Window's native sizing can replace its Width resource expression. Keep a
    // separate resource-backed property so later preference changes resize it.
    private static readonly System.Windows.DependencyProperty DiameterProperty = System.Windows.DependencyProperty.Register(
        "Diameter", typeof(double), typeof(LauncherWindow), new System.Windows.PropertyMetadata(52d, (owner, args) =>
        {
            var window = (LauncherWindow)owner;
            window.Width = window.Height = (double)args.NewValue;
        }));

    private static readonly nint HwndTopmost = new(-1);
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoActivate = 0x0010;
    private System.Windows.Point _mouseDownScreenPosition;
    private double _windowDownLeft;
    private double _windowDownTop;
    private bool _isPointerCaptured;
    private bool _wasDragged;
    private readonly DispatcherTimer _hoverTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    private bool _suppressHoverUntilLeave;
    private int _animationGeneration;
    private Action? _primaryClickAction;
    private bool _labelsOnRight;

    private LauncherViewModel ViewModel => (LauncherViewModel)DataContext;

    public LauncherWindow(LauncherViewModel viewModel)
    {
        InitializeComponent();
        SetResourceReference(DiameterProperty, "LauncherDiameter");
        DataContext = viewModel;
        ButtonsPopup.CustomPopupPlacementCallback = PlaceButtons;
        SettingsPopup.CustomPopupPlacementCallback = PlacePopup;
        NamePopup.CustomPopupPlacementCallback = PlaceName;
        viewModel.PropertyChanged += OnViewModelChanged;
        _hoverTimer.Tick += (_, _) =>
        {
            _hoverTimer.Stop();
            if (LauncherSurface.IsMouseOver && !_isPointerCaptured && !_suppressHoverUntilLeave)
                viewModel.ShowHiddenLists();
        };
        Closed += (_, _) =>
        {
            _hoverTimer.Stop();
            ++_animationGeneration;
            viewModel.PropertyChanged -= OnViewModelChanged;
            ButtonsPopup.IsOpen = false;
            SettingsPopup.IsOpen = false;
            NamePopup.IsOpen = false;
            ListMenuPopup.IsOpen = false;
        };
        Closing += OnClosing;
        Loaded += (_, _) => LauncherSurface.Focus();
    }

    public event Action<double, double>? PositionCommitted;

    public bool AllowClose { get; set; }

    public FrameworkElement MenuPreviewContent =>
        ButtonsPopup.Child as FrameworkElement
        ?? throw new InvalidOperationException("El menú no tiene contenido visual.");

    public FrameworkElement HiddenSubmenuPreviewContent =>
        ButtonsPopup.Child as FrameworkElement
        ?? throw new InvalidOperationException("El submenú no tiene contenido visual.");

    public FrameworkElement LauncherButtonPreviewContent => LauncherRoot;

    public void MinimizeLauncher()
    {
        DismissPanels();
        ++_animationGeneration;
        NamePopup.IsOpen = false;
        ButtonsPopup.IsOpen = false;
        _primaryClickAction = null;
        Hide();
    }

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
        if (!IsLoaded || !IsVisible)
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

        _hoverTimer.Stop();
        _primaryClickAction ??= ViewModel.PreparePrimaryClick();
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

        if (!_wasDragged) BeginLauncherDrag();
        _wasDragged = true;
        Left = _windowDownLeft + deltaX;
        Top = _windowDownTop + deltaY;
        e.Handled = true;
    }

    private void BeginLauncherDrag()
    {
        NamePopup.IsOpen = false;
        DismissPanels();
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
            (_primaryClickAction ?? viewModel.PreparePrimaryClick())();
        }
        _primaryClickAction = null;

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
        else if ((e.Key == Key.F10 && Keyboard.Modifiers == ModifierKeys.Shift) || e.Key == Key.Apps)
        {
            _hoverTimer.Stop();
            _suppressHoverUntilLeave = true;
            viewModel.ToggleMenu();
            FocusFirstPanelButton();
            e.Handled = true;
        }
        else if (e.Key == Key.Down && ReferenceEquals(Keyboard.FocusedElement, LauncherSurface))
        {
            viewModel.ShowHiddenLists();
            FocusFirstPanelButton();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            DismissPanels();
            LauncherSurface.Focus();
            e.Handled = true;
        }
        else if ((e.Key == Key.Enter || e.Key == Key.Space) &&
                 ReferenceEquals(Keyboard.FocusedElement, LauncherSurface))
        {
            viewModel.PrimaryClick();
            if (viewModel.IsPanelOpen) FocusFirstPanelButton();
            e.Handled = true;
        }
    }

    private void FocusFirstPanelButton()
    {
        Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
        {
            if (ViewModel.IsPanelOpen) FindFirstButton(ButtonsPanel)?.Focus();
        }));
    }

    private static Button? FindFirstButton(DependencyObject parent)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is FrameworkElement element && !element.IsVisible) continue;
            if (child is Button button) return button;
            if (FindFirstButton(child) is { } found) return found;
        }
        return null;
    }

    private void OnLauncherRightClick(object sender, MouseButtonEventArgs e)
    {
        _hoverTimer.Stop();
        _suppressHoverUntilLeave = true;
        ListMenuPopup.IsOpen = false;
        ViewModel.ToggleMenu();
        e.Handled = true;
    }

    private void OnHiddenListRightClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: HiddenTodoViewModel item } row) return;
        OpenListMenu(row, item);
        e.Handled = true;
    }

    private void OnHiddenListKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Apps && !(e.Key == Key.F10 && Keyboard.Modifiers == ModifierKeys.Shift)) return;
        if (sender is not FrameworkElement { DataContext: HiddenTodoViewModel item } row) return;
        OpenListMenu(row, item);
        e.Handled = true;
    }

    private void OpenListMenu(FrameworkElement row, HiddenTodoViewModel item)
    {
        _hoverTimer.Stop();
        ListMenuPopup.DataContext = item;
        ListMenuPopup.PlacementTarget = row is System.Windows.Controls.Grid grid
            ? grid.Children.OfType<System.Windows.Controls.Button>().FirstOrDefault() ?? row
            : row;
        ListMenuPopup.IsOpen = true;
        MoveListToTrashButton.Focus();
    }

    private void OnMoveListToTrash(object sender, RoutedEventArgs e)
    {
        var item = ListMenuPopup.DataContext as HiddenTodoViewModel;
        ListMenuPopup.IsOpen = false;
        item?.DeleteCommand.Execute(null);
        if (ViewModel.IsPanelOpen) FocusFirstPanelButton();
        else LauncherSurface.Focus();
    }

    public void CapturePrimaryClick() => _primaryClickAction = ViewModel.PreparePrimaryClick();

    public bool ContainsPrimaryButton(System.Windows.Point screenPoint) => ScreenInteraction.Contains(LauncherSurface, screenPoint);

    private void OnLauncherMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        NamePopup.IsOpen = !ViewModel.IsSettingsOpen;
        if (!_suppressHoverUntilLeave && !ViewModel.IsPanelOpen && !_isPointerCaptured)
            _hoverTimer.Start();
    }

    private void OnLauncherMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        NamePopup.IsOpen = false;
        _hoverTimer.Stop();
        _suppressHoverUntilLeave = false;
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        _hoverTimer.Stop();
        _suppressHoverUntilLeave = true;
        NamePopup.IsOpen = false;
        ListMenuPopup.IsOpen = false;
        ViewModel.IsSettingsOpen = true;
    }

    public bool ContainsInteraction(System.Windows.Point screenPoint) =>
        ScreenInteraction.Contains(LauncherSurface, screenPoint) ||
        (ButtonsPopup.IsOpen && ScreenInteraction.Contains(ButtonsPanel, screenPoint)) ||
        (SettingsPopup.IsOpen && ScreenInteraction.Contains(SettingsPopup.Child as FrameworkElement, screenPoint)) ||
        (ListMenuPopup.IsOpen && ScreenInteraction.Contains(ListMenuPopup.Child as FrameworkElement, screenPoint));

    public void DismissPanels()
    {
        ListMenuPopup.IsOpen = false;
        _hoverTimer.Stop();
        _suppressHoverUntilLeave = LauncherSurface.IsMouseOver;
        ViewModel.ClosePanels();
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LauncherViewModel.IsPanelOpen)) UpdateButtons();
    }

    private void UpdateButtons()
    {
        ListMenuPopup.IsOpen = false;
        var generation = ++_animationGeneration;
        if (ViewModel.IsPanelOpen)
        {
            ActionsPanel.Visibility = ViewModel.IsMenuOpen ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            HiddenPanel.Visibility = ViewModel.IsHiddenSubmenuOpen ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            var screenPoint = LauncherSurface.PointToScreen(new System.Windows.Point(0, 0));
            var workArea = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)screenPoint.X, (int)screenPoint.Y)).WorkingArea;
            var dpi = VisualTreeHelper.GetDpi(this);
            var above = (screenPoint.Y - workArea.Top) / dpi.DpiScaleY;
            var below = (workArea.Bottom - screenPoint.Y) / dpi.DpiScaleY - ActualHeight;
            _labelsOnRight = (screenPoint.X - workArea.Left) / dpi.DpiScaleX < ButtonsPanel.ActualWidth - ActualWidth;
            var direction = _labelsOnRight ? System.Windows.FlowDirection.RightToLeft : System.Windows.FlowDirection.LeftToRight;
            ActionsPanel.FlowDirection = direction;
            HiddenPanel.FlowDirection = direction;
            HiddenPanel.MaxHeight = Math.Max(64, Math.Max(above, below) - 16);
            ButtonsPopup.IsOpen = true;
            ButtonsPanel.UpdateLayout();
            AnimatePanel(1, 0);
        }
        else
        {
            AnimatePanel(0, 10);
            if (!AnimationSettings.AreAnimationsEnabled) { ButtonsPopup.IsOpen = false; return; }
            var closeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(160) };
            closeTimer.Tick += (_, _) =>
            {
                closeTimer.Stop();
                if (generation == _animationGeneration && !ViewModel.IsPanelOpen) ButtonsPopup.IsOpen = false;
            };
            closeTimer.Start();
        }
    }

    private void AnimatePanel(double opacity, double translation)
    {
        var duration = new System.Windows.Duration(TimeSpan.FromMilliseconds(AnimationSettings.AreAnimationsEnabled ? 150 : 0));
        var easing = new QuadraticEase { EasingMode = EasingMode.EaseOut };
        ButtonsPanel.BeginAnimation(OpacityProperty, new DoubleAnimation(opacity, duration) { EasingFunction = easing });
        PanelTranslation.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(translation, duration) { EasingFunction = easing });
    }

    private CustomPopupPlacement[] PlaceButtons(System.Windows.Size popupSize, System.Windows.Size targetSize, System.Windows.Point offset)
    {
        var x = _labelsOnRight ? 1 : targetSize.Width - popupSize.Width - 1;
        return
        [
            new(new System.Windows.Point(x, -popupSize.Height - 6), PopupPrimaryAxis.Horizontal),
            new(new System.Windows.Point(x, targetSize.Height + 6), PopupPrimaryAxis.Horizontal),
        ];
    }

    private static CustomPopupPlacement[] PlaceName(System.Windows.Size popupSize, System.Windows.Size targetSize, System.Windows.Point offset) =>
    [
        new(new System.Windows.Point(-popupSize.Width - 8, (targetSize.Height - popupSize.Height) / 2), PopupPrimaryAxis.Vertical),
        new(new System.Windows.Point(targetSize.Width + 8, (targetSize.Height - popupSize.Height) / 2), PopupPrimaryAxis.Vertical),
    ];

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
