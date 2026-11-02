using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Todos.Domain;
using Todos.Domain.Models;
using Todos.Presentation.ViewModels;
using Todos.Presentation.Views;

namespace Todos.Tests;

internal static class TaskPresentationTests
{
    [Test]
    public static void RealWpfViewAlignsInputAndShowsInsertionOnTheCorrectSide()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            System.Windows.Application? app = null;
            TodoWindow? window = null;
            try
            {
                app = new System.Windows.Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                foreach (var file in new[] { "DesignTokens.xaml", "SharedControls.xaml" })
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri($"pack://application:,,,/Todos;component/Presentation/Themes/{file}"),
                    });
                var state = new TodoWindowState { Title = "Agrupaciones de tareas" };
                var now = DateTimeOffset.UtcNow;
                TodoListOperations.AddTask(state, "Revisar los pasos de hoy", now);
                TodoListOperations.AddTask(state, "Prueba", now);
                var group = TodoListOperations.AddGroup(state, "Backend", now);
                TodoListOperations.AddTask(state, "Para verificador de precios: revisar productos y validar la respuesta", now, group.Id);
                TodoListOperations.AddTask(state, "Confirmar los cambios", now, group.Id);
                TodoListOperations.AddGroup(state, "Frontend", now);
                using var vm = new TodoWindowViewModel(state, () => { }, () => { }, () => { });
                window = new TodoWindow(vm) { Width = 560, Height = 660, ShowActivated = false, AllowClose = true };
                PlaceForReview(window);
                window.Show();
                Pump();
                vm.PendingTasks.First().IsCompleted = true; Pump();
                var clearButton = (Button)window.FindName("ClearCompletedButton");
                Assert.True(clearButton.IsVisible);
                if (Environment.GetEnvironmentVariable("TODOS_TEST_RENDER_DIR") is { } completedOutput)
                {
                    Directory.CreateDirectory(completedOutput);
                    Render(window, Path.Combine(completedOutput, "clear-completed.png"));
                }
                Assert.Equal(vm.ClearCompletedCommand, clearButton.Command);
                clearButton.Command.Execute(clearButton.CommandParameter);
                Pump();
                Assert.Equal(0, vm.CompletedCount);
                Assert.False(clearButton.IsVisible);
                vm.UndoDeleteCommand.Execute(null); Pump();
                vm.CompletedTasks.Single().IsCompleted = false; Pump();
                var backendGroup = vm.Groups.Single(item => item.Name == "Backend");
                Descendants<Button>(window).Single(button => button.DataContext == backendGroup && Equals(button.ToolTip, "Bajar grupo"))
                    .RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Pump();
                Assert.Equal(backendGroup, vm.Groups[2]);
                Descendants<Button>(window).Single(button => button.DataContext == backendGroup && Equals(button.ToolTip, "Subir grupo"))
                    .RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Pump();
                Assert.Equal(backendGroup, vm.Groups[1]);
                Descendants<Button>(window).Single(button => button.DataContext == backendGroup && Equals(button.ToolTip, "Renombrar grupo"))
                    .RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Pump();
                var groupEditor = Descendants<TextBox>(window).Single(box => box.DataContext == backendGroup && box.Name == "GroupTitleEditor");
                Assert.True(groupEditor.IsKeyboardFocused);
                Assert.Equal(groupEditor.Text.Length, groupEditor.SelectionLength);
                var editor = (TextBox)window.FindName("NewTaskEditor");
                editor.Focus();
                editor.Text = "T";
                window.UpdateLayout();
                var caret = editor.GetRectFromCharacterIndex(0);
                var hint = (TextBlock)editor.Template.FindName("InputHint", editor);
                hint.Visibility = Visibility.Visible;
                window.UpdateLayout();
                var hintX = hint.TranslatePoint(new Point(0, 0), editor).X;
                Assert.True(Math.Abs(caret.Left - hintX) <= 1, $"Caret {caret.Left}, placeholder {hintX}");
                editor.Text = string.Empty;
                hint.ClearValue(UIElement.VisibilityProperty);
                var rows = Descendants<Border>(window).Where(row => row.Name == "TaskRowContainer").ToArray();
                Assert.Equal(4, rows.Length);
                var row = rows[2];
                var cue = typeof(TodoWindow).GetMethod("ShowDropCue", BindingFlags.Instance | BindingFlags.NonPublic)!;
                cue.Invoke(window, [row, false]);
                Pump();
                var before = Descendants<Border>(row).Single(item => item.Name == "DropBeforeIndicator");
                var after = Descendants<Border>(row).Single(item => item.Name == "DropAfterIndicator");
                Assert.True(before.Opacity > 0.99 && after.Opacity < 0.01, "Upper half must show the BEFORE indicator.");
                cue.Invoke(window, [row, true]);
                Pump();
                Assert.True(after.Opacity > 0.99 && before.Opacity < 0.01, "Lower half must show the AFTER indicator.");
                typeof(TodoWindow).GetMethod("ClearDropCue", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(window, null);
                Pump();
                var output = Environment.GetEnvironmentVariable("TODOS_TEST_RENDER_DIR");
                if (output is not null)
                {
                    Directory.CreateDirectory(output);
                    Render(window, Path.Combine(output, "groups-560.png"));
                    window.Width = 410;
                    Pump();
                    Render(window, Path.Combine(output, "groups-410.png"));
                    window.Width = 280;
                    Pump();
                    Render(window, Path.Combine(output, "groups-280.png"));
                }

                window.Width = 560;
                Pump();
                var source = vm.Groups[0].Tasks[1];
                var firstTask = vm.Groups[0].Tasks[0];
                var dragged = typeof(TodoWindow).GetField("_draggedTask", BindingFlags.Instance | BindingFlags.NonPublic)!;
                dragged.SetValue(window, source);
                // The actual group background in the inter-row margin must resolve
                // to an insertion slot, including variable-height rows and side padding.
                var backendGroupBorder = Descendants<Border>(window).Single(item => item.Name == "TaskGroupContainer" && item.DataContext == vm.Groups[1]);
                var backendRows = Descendants<Border>(backendGroupBorder).Where(item => item.Name == "TaskRowContainer").ToArray();
                var gapY = (backendRows[0].TranslatePoint(new Point(), backendGroupBorder).Y + backendRows[0].ActualHeight +
                    backendRows[1].TranslatePoint(new Point(), backendGroupBorder).Y) / 2;
                vm.SelectThemeCommand.Execute(TodoColorTheme.Mint);
                Pump();
                for (var sample = 0; sample < 4; sample++)
                {
                    RaiseDrag(backendGroupBorder, source, DragDrop.DragOverEvent, gapY + sample * 0.2);
                    Assert.True(typeof(TodoWindow).GetField("_groupDropTarget", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(window) is null,
                        "Inter-row margins must not highlight the entire group.");
                    var gapCue = Descendants<Border>(backendRows[1]).Single(item => item.Name == "DropBeforeIndicator");
                    Assert.Equal(1d, gapCue.Opacity);
                    Assert.Equal((Color)ColorConverter.ConvertFromString(Todos.Presentation.Themes.TodoThemePalette.Get(TodoColorTheme.Mint).Accent),
                        ((SolidColorBrush)gapCue.Background).Color);
                }
                if (output is not null) Render(window, Path.Combine(output, "mint-gap-indicator.png"));
                RaiseDrag(backendGroupBorder, source, DragDrop.DropEvent, gapY);
                Pump();
                Assert.Equal(source.Id, vm.Groups[1].Tasks[1].Id);
                vm.MoveTaskToGroup(source.Id, null, 1);
                Pump();
                dragged.SetValue(window, source);
                typeof(TodoWindow).GetMethod("ClearDropCue", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(window, null);
                var targetRow = Descendants<Border>(window).Single(item => item.Name == "TaskRowContainer" && item.DataContext == firstTask);
                RaiseDrag(targetRow, source, DragDrop.DragOverEvent, 2);
                Pump();
                Assert.True(Descendants<Border>(targetRow).Single(item => item.Name == "DropBeforeIndicator").Opacity > 0.99);
                RaiseDrag(targetRow, source, DragDrop.DropEvent, 2);
                Pump();
                Assert.Equal(source.Id, vm.Groups[0].Tasks[0].Id);

                // Move down to the lower half: the cue and persisted placement agree.
                dragged.SetValue(window, source);
                targetRow = Descendants<Border>(window).Single(item => item.Name == "TaskRowContainer" && item.DataContext == firstTask);
                RaiseDrag(targetRow, source, DragDrop.DragOverEvent, targetRow.ActualHeight - 2);
                Pump();
                Assert.True(Descendants<Border>(targetRow).Single(item => item.Name == "DropAfterIndicator").Opacity > 0.99);
                RaiseDrag(targetRow, source, DragDrop.DropEvent, targetRow.ActualHeight - 2);
                Pump();
                Assert.Equal(source.Id, vm.Groups[0].Tasks[1].Id);

                // Cross-group insertion and an empty group use the same real routed events.
                dragged.SetValue(window, source);
                var backendFirst = vm.Groups[1].Tasks[0];
                targetRow = Descendants<Border>(window).Single(item => item.Name == "TaskRowContainer" && item.DataContext == backendFirst);
                RaiseDrag(targetRow, source, DragDrop.DragOverEvent, 2);
                RaiseDrag(targetRow, source, DragDrop.DropEvent, 2);
                Pump();
                Assert.Equal(source.Id, vm.Groups[1].Tasks[0].Id);
                dragged.SetValue(window, source);
                var emptyGroup = Descendants<Border>(window).First(item => item.DataContext == vm.Groups[2] && item.AllowDrop);
                RaiseDrag(emptyGroup, source, DragDrop.DragOverEvent, 12);
                RaiseDrag(emptyGroup, source, DragDrop.DropEvent, 12);
                Pump();
                Assert.Equal(source.Id, vm.Groups[2].Tasks[0].Id);

                typeof(TodoWindow).GetMethod("ShowDragPreview", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(window, [source]);
                Pump();
                var preview = (Grid)typeof(TodoWindow).GetField("_dragPreview", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(window)!;
                var originalRow = Descendants<Border>(window).Single(item => item.Name == "TaskRowContainer" && item.DataContext == source);
                Assert.False(preview.IsHitTestVisible);
                Assert.Equal(source.Text, System.Windows.Automation.AutomationProperties.GetName(preview));
                Assert.Equal(originalRow.ActualWidth, preview.Width);
                Assert.Equal(originalRow.ActualHeight, preview.Height);
                Assert.True(((Image)preview.Children[1]).Source is BitmapSource);
                var snapshot = (BitmapSource)((Image)preview.Children[1]).Source;
                var pixels = new byte[snapshot.PixelWidth * snapshot.PixelHeight * 4];
                snapshot.CopyPixels(pixels, snapshot.PixelWidth * 4, 0);
                Assert.True(Enumerable.Range(0, snapshot.PixelWidth * snapshot.PixelHeight).Count(index => pixels[index * 4 + 3] > 0) > 100,
                    "The drag snapshot must contain the real row, not an empty bitmap.");
                var mint = Todos.Presentation.Themes.TodoThemePalette.Get(TodoColorTheme.Mint);
                Assert.Equal((Color)ColorConverter.ConvertFromString(mint.Surface), ((SolidColorBrush)((Border)preview.Children[0]).Background).Color);
                Assert.Equal((Color)ColorConverter.ConvertFromString(mint.Accent), ((SolidColorBrush)((Border)preview.Children[2]).BorderBrush).Color);
                Canvas.SetLeft(preview, 30);
                Canvas.SetTop(preview, 260);
                Pump();
                if (output is not null) Render(preview, Path.Combine(output, "drag-card.png"));
                if (output is not null) Render(window, Path.Combine(output, "drag-preview-window.png"));
                ((Canvas)window.FindName("DragPreviewLayer")).Children.Clear();

                var header = (Grid)window.FindName("HeaderArea");
                var titleEditor = (TextBox)window.FindName("TitleEditor");
                Assert.True(titleEditor.IsReadOnly);
                Assert.True(Todos.Presentation.Windows.WindowHeaderInputPolicy.CanStartDrag(header, header, titleEditor));
                Assert.True(Todos.Presentation.Windows.WindowHeaderInputPolicy.CanStartDrag(titleEditor, header, titleEditor));
                Assert.False(Todos.Presentation.Windows.WindowHeaderInputPolicy.CanStartDrag((Button)window.FindName("PaletteButton"), header, titleEditor));
                Assert.False(Todos.Presentation.Windows.WindowHeaderInputPolicy.CanStartDrag(editor, header, titleEditor));
                titleEditor.IsReadOnly = false;
                Assert.False(Todos.Presentation.Windows.WindowHeaderInputPolicy.CanStartDrag(titleEditor, header, titleEditor));
                titleEditor.IsReadOnly = true;
                Assert.Equal(3, Descendants<Button>(header).Count());

                var captureButton = (Button)window.FindName("NewTaskGroupButton");
                Assert.True(Math.Abs(captureButton.TranslatePoint(new Point(), window).Y - editor.TranslatePoint(new Point(), window).Y) < 1,
                    "Group selection must share the input row.");
                Assert.Equal(0, Descendants<ComboBox>(window).Count());
                // Force WPF's actual hover state to exercise template triggers, including
                // dynamically changing the theme while the pointer stays on the control.
                var hoverKey = (DependencyPropertyKey)typeof(UIElement).GetField("IsMouseOverPropertyKey", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
                var pressedKey = (DependencyPropertyKey)typeof(System.Windows.Controls.Primitives.ButtonBase).GetField("IsPressedPropertyKey", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
                var addButton = Descendants<Button>(window).Single(button => button.Style == window.FindResource("AddTaskButtonStyle"));
                var appAccent = app.FindResource("TodosBrandBrush");
                editor.Text = "Comprobar hover";
                captureButton.SetValue(hoverKey, true);
                addButton.SetValue(hoverKey, true);
                foreach (var theme in Enum.GetValues<TodoColorTheme>())
                {
                    vm.SelectThemeCommand.Execute(theme);
                    Pump();
                    var groupSurface = (Border)captureButton.Template.FindName("Surface", captureButton);
                    var addSurface = (Border)addButton.Template.FindName("Surface", addButton);
                    Assert.Equal(window.FindResource("TodosHoverSurfaceBrush"), groupSurface.Background);
                    Assert.Equal(window.FindResource("TodosBrandHoverBrush"), addSurface.Background);
                    Assert.Equal(window.FindResource("TodosBrandBrush"), editor.SelectionBrush);
                    Assert.Equal(appAccent, app.FindResource("TodosBrandBrush"));
                    if (output is not null) Render(window, Path.Combine(output, $"theme-{theme}-hover.png"));
                    addButton.SetValue(pressedKey, true);
                    Assert.Equal(window.FindResource("TodosBrandPressedBrush"), addSurface.Background);
                    addButton.ClearValue(pressedKey);
                }
                captureButton.ClearValue(hoverKey);
                addButton.ClearValue(hoverKey);
                editor.Text = string.Empty;
                var groupHeader = Descendants<TextBox>(window).First(item => item.DataContext == vm.Groups[1]);
                vm.SelectThemeCommand.Execute(TodoColorTheme.Sky);
                Pump();
                var expectedAccent = (Color)ColorConverter.ConvertFromString(Todos.Presentation.Themes.TodoThemePalette.Get(TodoColorTheme.Sky).Accent);
                Assert.Equal(expectedAccent, ((SolidColorBrush)groupHeader.Foreground).Color);
                var groupContainer = Descendants<Border>(window).First(item => item.DataContext == vm.Groups[1] && item.AllowDrop);
                var toggle = Descendants<Button>(groupContainer).First(item => Equals(item.ToolTip, "Expandir o contraer grupo"));
                toggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Pump();
                Assert.False(vm.Groups[1].IsExpanded);
                Assert.False(Descendants<StackPanel>(groupContainer).Single(item => item.Name == "GroupContent").IsVisible);
                if (output is not null) Render(window, Path.Combine(output, "groups-sky-collapsed.png"));
                dragged.SetValue(window, source);
                RaiseDrag(groupContainer, source, DragDrop.DragOverEvent, 12);
                Assert.Equal(groupContainer, typeof(TodoWindow).GetField("_groupDropTarget", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(window));
                RaiseDrag(groupContainer, source, DragDrop.DropEvent, 12);
                Assert.Equal(source.Id, vm.Groups[1].Tasks.Last().Id);
                toggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                captureButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Pump();
                var groupMenu = (System.Windows.Controls.Primitives.Popup)window.FindName("GroupMenuPopup");
                Assert.True(groupMenu.IsOpen);
                var menuSurface = (Border)window.FindName("GroupMenuSurface");
                if (output is not null) Render(menuSurface, Path.Combine(output, "group-selector-menu.png"));
                var choice = Descendants<Button>(menuSurface).Single(button => button.DataContext == vm.Groups[1]);
                choice.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.False(groupMenu.IsOpen);
                Assert.Equal(vm.Groups[1], vm.SelectedGroup);

                Assert.False(Descendants<Button>(window).Any(button => Equals(button.ToolTip, "Mover a agrupación")));
                var movingRow = Descendants<Border>(window).First(item => item.Name == "TaskRowContainer");
                var movingTask = (TodoTaskViewModel)movingRow.DataContext;
                typeof(TodoWindow).GetMethod("OpenTaskGroupMenu", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(window, [movingRow, movingTask]);
                Pump();
                Assert.True(groupMenu.IsOpen);
                Assert.Equal(Visibility.Collapsed, ((Button)window.FindName("CreateGroupMenuButton")).Visibility);
                if (output is not null) Render(menuSurface, Path.Combine(output, "move-group-menu.png"));
                choice = Descendants<Button>(menuSurface).Single(button => button.DataContext == vm.Groups[1]);
                choice.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Equal(vm.Groups[1].Id, movingTask.GroupId);

                var remove = Descendants<Button>(window).First(button => button.DataContext == vm.Groups[1] &&
                    Equals(button.ToolTip, "Quitar grupo y mover sus tareas a General"));
                var oldCount = state.Tasks.Count;
                remove.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Pump();
                var confirmation = (System.Windows.Controls.Primitives.Popup)window.FindName("DeleteGroupPopup");
                Assert.True(confirmation.IsOpen);
                Assert.Equal(2, state.Groups.Count);
                if (output is not null) Render((Border)window.FindName("DeleteGroupSurface"), Path.Combine(output, "delete-group-confirmation.png"));
                ((Button)window.FindName("CancelDeleteGroupButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Equal(2, state.Groups.Count);
                remove.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Pump();
                Descendants<Button>((Border)window.FindName("DeleteGroupSurface")).Single(button => Equals(button.Content, "Eliminar"))
                    .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Equal(1, state.Groups.Count);
                Assert.Equal(oldCount, state.Tasks.Count);
                Assert.Equal<Guid?>(null, movingTask.GroupId);

                captureButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Pump();
                ((Button)window.FindName("CreateGroupMenuButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Pump();
                Assert.True(((Grid)window.FindName("GroupEditorPanel")).IsVisible);
                vm.NewGroupName = "Desde el menú";
                Descendants<Button>((Grid)window.FindName("GroupEditorPanel")).Single().RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.False(groupMenu.IsOpen);
                Assert.Equal("Desde el menú", vm.SelectedGroup!.Name);
                Assert.Equal(2, state.Groups.Count);
                VerifyLauncher(output);
                VerifyAutomaticMinimization();
                VerifyRecycleBin(output);
                VerifyAppearance(output);
                VerifySubtasks(output);
                VerifyNotes(output);
                VerifyOutsideClickRecovery(output);
            }
            catch (Exception exception) { Console.Error.WriteLine(exception); failure = exception; }
            finally { window?.Close(); app?.Shutdown(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null) throw failure;
    }

    private static System.Windows.Forms.Screen ReviewScreen => System.Windows.Forms.Screen.AllScreens.FirstOrDefault(screen => !screen.Primary) ?? System.Windows.Forms.Screen.PrimaryScreen!;
    private static Todos.Application.Windows.DisplayArea ReviewArea => (Todos.Application.Windows.DisplayArea)typeof(Todos.Infrastructure.Windows.WindowsDisplayService)
        .GetMethod("ConvertWorkingArea", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [ReviewScreen])!;
    private sealed class ReviewDisplayService : Todos.Application.Windows.IDisplayService
    {
        public Todos.Application.Windows.DisplayArea GetBestWorkingArea(double? x, double? y) => ReviewArea;
    }
    private static void PlaceForReview(System.Windows.Window window, double x = 40, double y = 30)
    {
        var area = ReviewArea;
        x = Math.Clamp(x, 0, Math.Max(0, area.Width - window.Width - 10));
        y = Math.Clamp(y, 0, Math.Max(0, area.Height - window.Height - 10));
        window.Left = area.Left + x; window.Top = area.Top + y;
        window.SourceInitialized += (_, _) =>
        {
            var dpi = VisualTreeHelper.GetDpi(window);
            var bounds = ReviewScreen.WorkingArea;
            SetWindowPos(new System.Windows.Interop.WindowInteropHelper(window).Handle, IntPtr.Zero,
                bounds.Left + (int)(x * dpi.DpiScaleX), bounds.Top + (int)(y * dpi.DpiScaleY), 0, 0, 0x0015);
        };
        window.Loaded += (_, _) => Assert.Equal(ReviewScreen.DeviceName,
            System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(window).Handle).DeviceName);
    }
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int cx, int cy, uint flags);

    private static void VerifyOutsideClickRecovery(string? output)
    {
        using var directory = new PersistenceTests.TemporaryDirectory();
        var state = new AppState { Settings = new AppSettings { AutoMinimizeWindows = true } };
        var list = new TodoWindowState { Title = "Recuperación de clics", PositionX = ReviewArea.Left + 40, PositionY = ReviewArea.Top + 30 };
        state.TodoWindows.Add(list);
        var saver = new Todos.Application.Persistence.DebouncedAppStateSaver(new ImmediateRepository());
        using var controller = new Todos.Presentation.AppController(state, saver, null,
            new Todos.Application.AppLaunchOptions(false, false, directory.Path, null, null, false, 350), new InertStartup(),
            new Todos.Infrastructure.Logging.LocalLogService(new Todos.Infrastructure.Persistence.AppDataPaths(directory.Path)));
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var manager = (Todos.Presentation.Windows.TodoWindowManager)typeof(Todos.Presentation.AppController).GetField("_windowManager", flags)!.GetValue(controller)!;
        var launcherVm = new LauncherViewModel(false, () => { }, () => { }, () => { }, _ => { }, _ => { }, _ => true, () => Task.CompletedTask);
        var launcher = new LauncherWindow(launcherVm) { AllowClose = true };
        PlaceForReview(launcher, 650, 420);
        typeof(Todos.Presentation.AppController).GetField("_launcherWindow", flags)!.SetValue(controller, launcher);
        typeof(Todos.Presentation.AppController).GetField("_launcherViewModel", flags)!.SetValue(controller, launcherVm);
        var outside = typeof(Todos.Presentation.AppController).GetMethod("OnOutsideClick", flags)!;
        using var monitor = new Todos.Infrastructure.Windows.OutsideClickMonitor((_, _) => () => { });
        typeof(Todos.Presentation.AppController).GetField("_outsideClicks", flags)!.SetValue(controller, monitor);
        var prepare = typeof(Todos.Infrastructure.Windows.OutsideClickMonitor).GetMethod("PreparePendingClick", flags)!;
        Action Guard(Action action) => (Action)prepare.Invoke(monitor, [(Func<Action>)(() => action)])!;
        var applied = 0;
        var stale = Guard(() => applied++); monitor.InvalidatePendingClicks(); stale(); Assert.Equal(0, applied);
        var interrupted = (Action)prepare.Invoke(monitor, [(Func<Action>)(() => { monitor.InvalidatePendingClicks(); return () => applied++; })])!;
        interrupted(); Assert.Equal(0, applied);
        try
        {
            launcher.Show(); manager.Show(list); Pump();
            var window = manager.GetWindow(list.Id)!;
            var point = PhysicalCenter(window);
            var results = new List<string>();
            foreach (var dpiContext in new[] { -1, -2, -3 })
            {
                var previous = SetThreadDpiAwarenessContext(new IntPtr(dpiContext));
                Assert.True(previous != IntPtr.Zero);
                try
                {
                    Assert.True(Todos.Presentation.Windows.ScreenInteraction.Contains(window, point));
                    ((Action)outside.Invoke(controller, [point])!)(); Assert.True(list.IsVisible);
                    var local = window.PointFromScreen(point);
                    results.Add($"DPI context {dpiContext}: native internal=True; former geometric internal={local.X >= 0 && local.Y >= 0 && local.X <= window.ActualWidth && local.Y <= window.ActualHeight}");
                    Assert.True(AreDpiAwarenessContextsEqual(GetThreadDpiAwarenessContext(), new IntPtr(dpiContext)));
                }
                finally { SetThreadDpiAwarenessContext(previous); }
            }
            var nativeBounds = PhysicalBounds(window);
            var windowHandle = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            var resizeContext = SetThreadDpiAwarenessContext(new IntPtr(-3));
            try
            {
                // A native resize can precede WPF's next layout pass during recovery.
                SetWindowPos(windowHandle, IntPtr.Zero, 0, 0, nativeBounds.Right - nativeBounds.Left + 120,
                    nativeBounds.Bottom - nativeBounds.Top, 0x0016);
                var resized = PhysicalBounds(window);
                var edgePoint = new Point(resized.Right - 8, (resized.Top + resized.Bottom) / 2d);
                var oldLocal = window.PointFromScreen(edgePoint);
                var oldInside = oldLocal.X >= 0 && oldLocal.Y >= 0 && oldLocal.X <= window.ActualWidth && oldLocal.Y <= window.ActualHeight;
                Assert.True(window.ContainsInteraction(edgePoint));
                ((Action)outside.Invoke(controller, [edgePoint])!)(); Assert.True(list.IsVisible);
                results.Add($"Native resize before WPF layout: native internal=True; former geometric internal={oldInside}");
                SetWindowPos(windowHandle, IntPtr.Zero, 0, 0, nativeBounds.Right - nativeBounds.Left,
                    nativeBounds.Bottom - nativeBounds.Top, 0x0016);
            }
            finally { SetThreadDpiAwarenessContext(resizeContext); }
            Pump();
            var buttonPoint = PhysicalCenter((Border)launcher.FindName("LauncherSurface"));
            var openingClick = (Action)outside.Invoke(controller, [buttonPoint])!;
            launcherVm.IsMenuOpen = true; openingClick(); Pump();
            Assert.True(launcherVm.IsMenuOpen, "The delayed right-click check must not dismiss the actions it just opened.");
            var popup = (System.Windows.Controls.Primitives.Popup)launcher.FindName("ButtonsPopup");
            var popupPoint = PhysicalCenter((FrameworkElement)popup.Child);
            Assert.True(launcher.ContainsInteraction(popupPoint));
            var previousContext = SetThreadDpiAwarenessContext(new IntPtr(-1));
            try { ((Action)outside.Invoke(controller, [popupPoint])!)(); Assert.True(launcherVm.IsMenuOpen); }
            finally { SetThreadDpiAwarenessContext(previousContext); }
            manager.Show(list); Pump();
            var obsolete = Guard((Action)outside.Invoke(controller, [new Point(-10000, -10000)])!);
            typeof(Todos.Presentation.AppController).GetMethod("OnPowerModeChanged", flags)!.Invoke(controller,
                [controller, new Microsoft.Win32.PowerModeChangedEventArgs(Microsoft.Win32.PowerModes.Resume)]);
            Pump(); obsolete(); Assert.True(list.IsVisible); Assert.True(launcherVm.IsMenuOpen);
            var beforeDisplayChange = Guard(() => applied++);
            typeof(Todos.Presentation.AppController).GetMethod("OnDisplaySettingsChanged", flags)!.Invoke(controller, [null, EventArgs.Empty]);
            Pump(); beforeDisplayChange(); Assert.Equal(0, applied);
            Guard(() => applied++)(); Assert.Equal(1, applied);
            Assert.True(Todos.Presentation.Windows.ScreenInteraction.Contains(window, PhysicalCenter(window)));
            ((Action)outside.Invoke(controller, [new Point(-10000, -10000)])!)();
            Assert.False(list.IsVisible); Assert.False(launcherVm.IsMenuOpen);
            var afterDispose = Guard(() => applied++); monitor.Dispose(); afterDispose(); Assert.Equal(1, applied);
            if (output is not null) File.WriteAllLines(Path.Combine(output, "click-recovery.txt"), results);
        }
        finally { launcher.Close(); }
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct PhysicalRect { public int Left, Top, Right, Bottom; }
    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern IntPtr GetThreadDpiAwarenessContext();
    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern bool AreDpiAwarenessContextsEqual(IntPtr first, IntPtr second);
    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr window, out PhysicalRect bounds);
    private static Point PhysicalCenter(FrameworkElement element)
    {
        var bounds = PhysicalBounds(element);
        return new Point((bounds.Left + bounds.Right) / 2d, (bounds.Top + bounds.Bottom) / 2d);
    }
    private static PhysicalRect PhysicalBounds(FrameworkElement element)
    {
        var source = (System.Windows.Interop.HwndSource)PresentationSource.FromVisual(element)!;
        var previous = SetThreadDpiAwarenessContext(new IntPtr(-3));
        try
        {
            Assert.True(GetWindowRect(source.Handle, out var bounds));
            return bounds;
        }
        finally { SetThreadDpiAwarenessContext(previous); }
    }

    private static void VerifyNotes(string? output)
    {
        Todos.Presentation.Themes.AppearanceResources.Apply(new AppSettings());
        var state = new TodoWindowState { Title = "Notas del proyecto", ColorTheme = TodoColorTheme.Sky };
        var root = TodoListOperations.AddTask(state, "Revisar la API", DateTimeOffset.UtcNow);
        SubtaskOperations.Add(state, root.Id, "Revisar endpoints", DateTimeOffset.UtcNow);
        using var vm = new TodoWindowViewModel(state, () => { }, () => { }, () => { });
        var window = new TodoWindow(vm) { AllowClose = true, Width = 520, Height = 600 };
        PlaceForReview(window);
        try
        {
            window.Show(); Pump();
            var open = typeof(TodoWindow).GetMethod("OpenNotes", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var notesField = typeof(TodoWindow).GetField("_notesWindows", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var windows = (Dictionary<Guid, TaskNotesWindow>)notesField.GetValue(window)!;
            var rootVm = vm.PendingTasks.Single(); var childVm = rootVm.Subtasks.Single();
            open.Invoke(window, [rootVm]); Pump();
            var note = windows[root.Id];
            Assert.Equal(ReviewScreen.DeviceName, System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(note).Handle).DeviceName);
            var tabs = (TabControl)note.FindName("NotesTabs"); var editor = (TextBox)note.FindName("NoteEditor");
            Assert.Equal(1, tabs.SelectedIndex); Assert.True(editor.IsKeyboardFocused);
            editor.Text = NoteTests.Markdown; Pump(); Assert.Equal(NoteTests.Markdown, root.NotesMarkdown);
            Assert.Equal("Guardando…", rootVm.NotesSaveStatus);
            if (output is not null) Render(note, Path.Combine(output, "notes-editor.png"));
            tabs.SelectedIndex = 0; Pump();
            var preview = (System.Windows.Controls.FlowDocumentScrollViewer)note.FindName("NotePreview");
            var text = new System.Windows.Documents.TextRange(preview.Document.ContentStart, preview.Document.ContentEnd).Text;
            Assert.True(text.Contains("Módulo API") && text.Contains("☑") && text.Contains("☐") && text.Contains("$id = 1;"));
            Assert.True(preview.Document.Blocks.OfType<System.Windows.Documents.Table>().Any());
            Assert.True(text.Contains("[Imagen: Referencia]"));
            var point = PhysicalCenter(note); Assert.True(window.ContainsInteraction(point));
            if (output is not null) Render(note, Path.Combine(output, "notes-preview.png"));
            open.Invoke(window, [rootVm]); Pump(); Assert.Equal(1, windows.Count);
            typeof(TaskNotesWindow).GetMethod("StageImport", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(note, ["# Reemplazo"]); Pump();
            Assert.Equal(NoteTests.Markdown, rootVm.NotesMarkdown);
            ((Button)note.FindName("CancelImportButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(NoteTests.Markdown, rootVm.NotesMarkdown);
            typeof(TaskNotesWindow).GetMethod("StageImport", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(note, ["# Reemplazo"]);
            ((Button)note.FindName("ConfirmImportButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Pump();
            Assert.Equal("# Reemplazo", rootVm.NotesMarkdown); Assert.Equal(1, tabs.SelectedIndex);
            rootVm.NotesMarkdown = NoteTests.Markdown;
            note.Close(); open.Invoke(window, [rootVm]); Pump(); note = windows[root.Id];
            tabs = (TabControl)note.FindName("NotesTabs"); Assert.Equal(0, tabs.SelectedIndex);
            var defaultFont = ((System.Windows.Controls.FlowDocumentScrollViewer)note.FindName("NotePreview")).Document.FontSize;
            Todos.Presentation.Themes.AppearanceResources.Apply(new AppSettings { FontSizeStep = 4 }); Pump();
            Assert.True(((System.Windows.Controls.FlowDocumentScrollViewer)note.FindName("NotePreview")).Document.FontSize > defaultFont);
            vm.SelectThemeCommand.Execute(TodoColorTheme.Rose); Pump();
            Assert.Equal(((System.Windows.Media.SolidColorBrush)window.Resources["TodosPopupBrush"]).Color, ((System.Windows.Media.SolidColorBrush)note.Resources["TodosPopupBrush"]).Color);
            Todos.Presentation.Themes.AppearanceResources.Apply(new AppSettings());
            open.Invoke(window, [childVm]); Pump(); Assert.Equal(2, windows.Count);
            childVm.NotesMarkdown = "Nota propia del paso";
            if (output is not null) Render(window, Path.Combine(output, "notes-shortcuts.png"));
            childVm.DeleteCommand.Execute(null); Pump(); Assert.False(windows.ContainsKey(childVm.Id));
            vm.UndoDeleteCommand.Execute(null); Pump(); Assert.Equal("Nota propia del paso", vm.PendingTasks.Single().Subtasks.Single().NotesMarkdown);
            var openMenu = typeof(TodoWindow).GetMethod("OpenTaskGroupMenu", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var rootRow = Descendants<Border>(window).Single(row => row.Name == "TaskRowContainer" && ReferenceEquals(row.DataContext, rootVm));
            openMenu.Invoke(window, [rootRow, rootVm]); Pump();
            Assert.Equal("Ver nota", ((Button)window.FindName("NotesMenuButton")).Content);
            var child = vm.PendingTasks.Single().Subtasks.Single();
            var childRow = Descendants<Border>(window).Single(row => row.Name == "SubtaskRow" && ReferenceEquals(row.DataContext, child));
            openMenu.Invoke(window, [childRow, child]); Pump(); Assert.False(rootVm.IsContextTarget); Assert.True(child.IsContextTarget);
            openMenu.Invoke(window, [rootRow, rootVm]); Pump(); Assert.True(rootVm.IsContextTarget); Assert.False(child.IsContextTarget);
            window.Hide(); Pump(); Assert.Equal(0, windows.Count); Assert.Equal(NoteTests.Markdown, rootVm.NotesMarkdown);
        }
        finally { window.Close(); Todos.Presentation.Themes.AppearanceResources.Apply(new AppSettings()); }
    }

    private static void VerifySubtasks(string? output)
    {
        var state = new TodoWindowState { Title = "Implementar módulo de pedidos", AreCompletedTasksExpanded = true };
        var now = DateTimeOffset.UtcNow;
        var root = TodoListOperations.AddTask(state, "Entregar módulo", now);
        var first = SubtaskOperations.Add(state, root.Id, "Crear endpoints", now);
        var second = SubtaskOperations.Add(state, root.Id, "Implementar la vista de captura", now);
        TodoListOperations.CompleteTask(state, first.Id, now);
        var source = TodoListOperations.AddTask(state, "Pruebas manuales", now);
        using var vm = new TodoWindowViewModel(state, () => { }, () => { }, () => { });
        var window = new TodoWindow(vm) { Width = 480, Height = 680, AllowClose = true };
        try
        {
            PlaceForReview(window); window.Show(); Pump();
            Assert.Equal(2, vm.PendingCount);
            var rootVm = vm.PendingTasks.Single(task => task.Id == root.Id);
            Assert.Equal("1/2", rootVm.SubtaskProgress);
            Assert.Equal(2, Descendants<Border>(window).Count(row => row.Name == "SubtaskRow"));
            var rootRow = Descendants<Border>(window).Single(row => row.Name == "TaskRowContainer" && row.DataContext == rootVm);
            var context = typeof(TodoWindow).GetMethod("OpenTaskGroupMenu", BindingFlags.Instance | BindingFlags.NonPublic)!;
            context.Invoke(window, [rootRow, rootVm]); Pump();
            ((Button)window.FindName("AddSubtaskMenuButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Pump();
            Thread.Sleep(160); Pump();
            var capture = Descendants<TextBox>(window).Single(box => box.Name == "SubtaskEditor" && box.DataContext == rootVm);
            Assert.True(capture.IsVisible, "Subtask capture must become visible.");
            Assert.True(capture.IsKeyboardFocused, "Subtask capture must receive keyboard focus.");
            capture.Text = "Verificar sincronización sin conexión";
            typeof(TodoWindow).GetMethod("OnNewSubtaskKeyDown", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(window,
                [capture, new System.Windows.Input.KeyEventArgs(System.Windows.Input.Keyboard.PrimaryDevice,
                    System.Windows.PresentationSource.FromVisual(window), 0, System.Windows.Input.Key.Enter) { RoutedEvent = System.Windows.Input.Keyboard.KeyDownEvent }]);
            Pump();
            Assert.Equal(3, root.Subtasks.Count);
            Assert.Equal(string.Empty, rootVm.NewSubtaskText);
            rootVm.EndSubtaskCapture(); Pump();
            if (output is not null) Render(window, Path.Combine(output, "subtasks-expanded.png"));
            rootVm.ToggleSubtasksCommand.Execute(null); Pump();
            Assert.False(root.AreSubtasksExpanded);
            if (output is not null) Render(window, Path.Combine(output, "subtasks-collapsed.png"));
            rootVm.ToggleSubtasksCommand.Execute(null); Pump();
            // Exercise the real routed drag/drop handlers with a typed payload.
            var target = Descendants<Border>(window).Single(row => row.Name == "SubtaskRow" && row.DataContext is TodoTaskViewModel task && task.Id == first.Id);
            var dragType = typeof(TodoWindow).GetNestedType("SubtaskDragItem", BindingFlags.NonPublic)!;
            var payload = Activator.CreateInstance(dragType, [second.Id, root.Id])!;
            var data = new System.Windows.DataObject(); data.SetData(dragType, payload);
            DragEventArgs MakeDrag(RoutedEvent routedEvent)
            {
                var args = (DragEventArgs)Activator.CreateInstance(typeof(DragEventArgs), BindingFlags.Instance | BindingFlags.NonPublic, null,
                    [data, DragDropKeyStates.LeftMouseButton, DragDropEffects.Move, target, new Point(5, 1)], null)!;
                args.RoutedEvent = routedEvent; return args;
            }
            var drag = MakeDrag(System.Windows.DragDrop.DragOverEvent);
            target.RaiseEvent(drag); Assert.Equal(System.Windows.DragDropEffects.Move, drag.Effects);
            var indicator = Descendants<Border>(target).Single(item => item.Name == "DropBeforeIndicator");
            var height = target.ActualHeight;
            for (var sample = 0; sample < 8; sample++)
            {
                target.RaiseEvent(MakeDrag(System.Windows.DragDrop.DragOverEvent));
                target.RaiseEvent(MakeDrag(System.Windows.DragDrop.DragLeaveEvent));
                Assert.Equal(1d, indicator.Opacity);
                Assert.Equal(new CornerRadius(0), indicator.CornerRadius);
                Assert.Equal(new Thickness(0), target.BorderThickness);
                Assert.Equal(height, target.ActualHeight);
            }
            var sourceStepRow = Descendants<Border>(window).Single(row => row.Name == "SubtaskRow" && row.DataContext is TodoTaskViewModel task && task.Id == second.Id);
            var stepVm = (TodoTaskViewModel)sourceStepRow.DataContext;
            var begin = typeof(TodoWindow).GetMethod("BeginRowDrag", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var end = typeof(TodoWindow).GetMethod("EndRowDrag", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var originalOpacity = sourceStepRow.ReadLocalValue(UIElement.OpacityProperty);
            begin.Invoke(window, [sourceStepRow, stepVm]); Pump();
            var card = (Grid)typeof(TodoWindow).GetField("_dragPreview", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(window)!;
            Assert.Equal(.3, sourceStepRow.Opacity); Assert.False(card.IsHitTestVisible);
            Assert.True(Math.Abs(card.Width - sourceStepRow.ActualWidth) < 1);
            Assert.True(Descendants<Image>(card).Single().Source is BitmapSource bitmap && bitmap.PixelWidth > 0);
            var captured = (BitmapSource)Descendants<Image>(card).Single().Source;
            var pixels = new byte[captured.PixelWidth * captured.PixelHeight * 4];
            captured.CopyPixels(pixels, captured.PixelWidth * 4, 0);
            Assert.True(Enumerable.Range(0, captured.PixelWidth * captured.PixelHeight).Count(index => pixels[index * 4 + 3] > 0) > 100);
            Canvas.SetLeft(card, 30); Canvas.SetTop(card, 300); Pump();
            if (output is not null)
            {
                Render(card, Path.Combine(output, "subtask-drag-preview.png"));
                Render(window, Path.Combine(output, "subtask-drag-source.png"));
            }
            end.Invoke(window, null);
            Assert.Equal(originalOpacity, sourceStepRow.ReadLocalValue(UIElement.OpacityProperty));
            Assert.Equal(0, ((Canvas)window.FindName("DragPreviewLayer")).Children.Count);
            Assert.Equal(0d, indicator.Opacity);
            var surface = Descendants<Border>(window).Single(row => row.Name == "SubtaskDropSurface" && ReferenceEquals(row.DataContext, rootVm));
            var firstTop = target.TranslatePoint(new Point(), surface).Y;
            // The margin above a row remains an insertion slot for both hover and drop.
            DragEventArgs GapDrag(RoutedEvent routedEvent)
            {
                var args = (DragEventArgs)Activator.CreateInstance(typeof(DragEventArgs), BindingFlags.Instance | BindingFlags.NonPublic, null,
                    [data, DragDropKeyStates.LeftMouseButton, DragDropEffects.Move, surface, new Point(5, Math.Max(0, firstTop - .5))], null)!;
                args.RoutedEvent = routedEvent; return args;
            }
            for (var sample = 0; sample < 5; sample++) surface.RaiseEvent(GapDrag(DragDrop.DragOverEvent));
            Assert.Equal(1d, indicator.Opacity);
            surface.RaiseEvent(GapDrag(DragDrop.DropEvent)); Pump();
            Assert.Equal(second.Id, root.Subtasks[0].Id);
            var sourceVm = vm.PendingTasks.Single(task => task.Id == source.Id);
            target = Descendants<Border>(window).First(row => row.Name == "SubtaskRow");
            typeof(TodoWindow).GetField("_draggedTask", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(window, sourceVm);
            RaiseDrag(target, sourceVm, System.Windows.DragDrop.DragOverEvent, 1);
            RaiseDrag(target, sourceVm, System.Windows.DragDrop.DropEvent, 1); Pump();
            Assert.Equal(2, vm.PendingCount);
            Assert.Equal(3, root.Subtasks.Count);
            Assert.Equal<Guid?>(null, sourceVm.ParentId);
            var sourceRow = Descendants<Border>(window).Single(row => row.Name == "TaskRowContainer" && row.DataContext == sourceVm);
            context.Invoke(window, [sourceRow, sourceVm]); Pump();
            Assert.True(sourceVm.IsContextTarget);
            Assert.Equal("Editar tarea", ((Button)window.FindName("EditTaskMenuButton")).Content);
            Assert.True(window.FindName("ConvertSubtaskMenuButton") is null);
            ((Button)window.FindName("EditTaskMenuButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Pump();
            Assert.True(sourceVm.IsEditing); Assert.False(sourceVm.IsContextTarget);
            sourceVm.CancelEdit();
            rootVm = vm.PendingTasks.Single(task => task.Id == root.Id);
            var inline = Descendants<ContentControl>(window).Single(control => control.Name == "InlineSubtaskProgress" && ReferenceEquals(control.DataContext, rootVm));
            var checkbox = Descendants<CheckBox>(window).First(control => ReferenceEquals(control.DataContext, rootVm));
            Assert.True(Math.Abs(inline.TranslatePoint(new Point(), window).Y - checkbox.TranslatePoint(new Point(), window).Y) < 20);
            var cursorPoint = new Point(45, 15);
            var menuRoot = Descendants<Border>(window).Single(row => row.Name == "TaskRowContainer" && ReferenceEquals(row.DataContext, rootVm));
            typeof(TodoWindow).GetMethod("OpenGroupMenu", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(window, [menuRoot, rootVm, cursorPoint]); Pump();
            var menu = (System.Windows.Controls.Primitives.Popup)window.FindName("GroupMenuPopup");
            Assert.Equal(System.Windows.Controls.Primitives.PlacementMode.RelativePoint, menu.Placement);
            Assert.Equal(53d, menu.HorizontalOffset); Assert.Equal(21d, menu.VerticalOffset);
            Assert.True(rootVm.IsContextTarget);
            var origin = ((FrameworkElement)menu.Child).PointToScreen(new Point());
            var clicked = menuRoot.PointToScreen(cursorPoint);
            Assert.True(Math.Abs(origin.X - clicked.X) < 320 && Math.Abs(origin.Y - clicked.Y) < 120);
            Assert.True(Descendants<TextBlock>((DependencyObject)window.FindName("GroupMenuItems")).Any(text => text.Text.StartsWith("Mover a ")));
            if (output is not null)
            {
                RenderLauncher((FrameworkElement)menu.Child, Path.Combine(output, "task-context-menu.png"));
                Render(window, Path.Combine(output, "task-context-focus.png"));
            }
            ((Button)window.FindName("CompleteTaskMenuButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Pump();
            Assert.False(rootVm.IsContextTarget);
            Assert.Equal(1, vm.PendingCount); Assert.Equal(1, vm.CompletedCount);
            Assert.Equal("3/3", vm.CompletedTasks[0].SubtaskProgress);
            if (output is not null) Render(window, Path.Combine(output, "subtasks-completed.png"));
            vm.UndoDeleteCommand.Execute(null); Pump();
            Assert.Equal(2, vm.PendingCount);
            rootVm = vm.PendingTasks.Single(task => task.Id == root.Id);
            Assert.Equal("1/3", rootVm.SubtaskProgress);
            var childRow = Descendants<Border>(window).First(row => row.Name == "SubtaskRow");
            var childVm = (TodoTaskViewModel)childRow.DataContext;
            context.Invoke(window, [childRow, childVm]); Pump();
            Assert.True(childVm.IsContextTarget);
            Assert.Equal("Editar subtarea", ((Button)window.FindName("EditTaskMenuButton")).Content);
            Assert.Equal(Visibility.Collapsed, ((ItemsControl)window.FindName("GroupMenuItems")).Visibility);
            if (output is not null) RenderLauncher((FrameworkElement)menu.Child, Path.Combine(output, "subtask-context-menu.png"));
            ((Button)window.FindName("DeleteTaskMenuButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Pump();
            Assert.False(childVm.IsContextTarget); Assert.Equal(2, root.Subtasks.Count);
            vm.UndoDeleteCommand.Execute(null); Assert.Equal(3, root.Subtasks.Count);
            vm.ClearCompletedCommand.Execute(null);
            Assert.Equal(3, root.Subtasks.Count);
        }
        finally { window.Close(); }
    }

    private static void VerifyAppearance(string? output)
    {
        var settings = new AppSettings();
        var vm = new LauncherViewModel(false, () => { }, () => { }, () => { }, _ => { }, _ => { }, _ => true,
            () => Task.CompletedTask, appearanceSettings: settings,
            appearanceChanged: () => Todos.Presentation.Themes.AppearanceResources.Apply(settings));
        var launcher = new LauncherWindow(vm) { AllowClose = true };
        PlaceForReview(launcher, 600, 200);
        var state = new TodoWindowState { Title = "Preferencias de apariencia", AreCompletedTasksExpanded = true };
        TodoListOperations.AddTask(state, "Revisar una tarea larga que puede ocupar varias líneas al aumentar el tamaño de letra.", DateTimeOffset.UtcNow);
        var group = TodoListOperations.AddGroup(state, "Backend", DateTimeOffset.UtcNow);
        TodoListOperations.AddTask(state, "Comprobar la configuración", DateTimeOffset.UtcNow, group.Id);
        using var tasks = new TodoWindowViewModel(state, () => { }, () => { }, () => { });
        var taskWindow = new TodoWindow(tasks) { Width = 410, Height = 620, AllowClose = true };
        var binVm = new RecycleBinViewModel(_ => { }, _ => { });
        binVm.Refresh([state]);
        var bin = new RecycleBinWindow(binVm);
        try
        {
            PlaceForReview(taskWindow); PlaceForReview(bin, 620, 50);
            launcher.Show(); taskWindow.Show(); bin.Show(); Pump();
            var taskText = Descendants<TextBlock>(taskWindow).First(item => item.Text.StartsWith("Revisar una tarea"));
            Assert.Equal(14d, taskText.FontSize);
            Assert.Equal(52d, launcher.ActualWidth);
            vm.IsSettingsOpen = true; Pump();
            var popup = (System.Windows.Controls.Primitives.Popup)launcher.FindName("SettingsPopup");
            Assert.Equal(6, Descendants<RadioButton>(popup.Child).Count());
            foreach (var step in Enumerable.Range(0, 4)) vm.LargerFontCommand.Execute(null);
            vm.LauncherLarge = true; vm.SpacingLarge = true; Pump();
            launcher.UpdateLayout(); Thread.Sleep(100); Pump();
            Assert.True(Math.Abs(taskText.FontSize - 19.6) < .01);
            Assert.Equal(68d, launcher.ActualWidth);
            var row = Descendants<Border>(taskWindow).First(item => item.Name == "TaskRowContainer");
            var largeMargin = row.Margin.Top;
            if (output is not null)
            {
                RenderLauncher((FrameworkElement)popup.Child, Path.Combine(output, "appearance-large-settings.png"));
                RenderLauncher(taskWindow, Path.Combine(output, "appearance-large-tasks.png"));
                RenderLauncher(bin, Path.Combine(output, "appearance-large-bin.png"));
            }
            for (var index = 0; index < 6; index++) vm.SmallerFontCommand.Execute(null);
            vm.LauncherSmall = true; vm.SpacingSmall = true; Pump();
            launcher.UpdateLayout(); Thread.Sleep(100); Pump();
            Assert.Equal(40d, launcher.ActualWidth);
            Assert.True(Math.Abs(taskText.FontSize - 11.2) < .01);
            Assert.True(row.Margin.Top < largeMargin);
            if (output is not null) RenderLauncher(taskWindow, Path.Combine(output, "appearance-small-tasks.png"));
            vm.SpacingMedium = true; vm.LauncherMedium = true;
            for (var index = 0; index < 2; index++) vm.LargerFontCommand.Execute(null);
            Pump();
            launcher.UpdateLayout(); Thread.Sleep(100); Pump();
            Assert.Equal(14d, taskText.FontSize);
            Assert.Equal(52d, launcher.ActualWidth);
        }
        finally
        {
            bin.Close(); taskWindow.Close(); launcher.Close();
            Todos.Presentation.Themes.AppearanceResources.Apply(new AppSettings());
        }
    }

    private static void VerifyAutomaticMinimization()
    {
        using var directory = new PersistenceTests.TemporaryDirectory();
        var state = new AppState();
        state.Settings.AutoMinimizeWindows = true;
        var recovering = new TodoWindowState { Title = "Recuperar", IsVisible = false };
        var visible = new TodoWindowState { Title = "Otra lista", PositionX = ReviewArea.Left + 40, PositionY = ReviewArea.Top + 30 };
        recovering.PositionX = ReviewArea.Left + 40; recovering.PositionY = ReviewArea.Top + 30;
        TodoListOperations.AddTask(recovering, "Nota y guardado", DateTimeOffset.UtcNow);
        state.TodoWindows.AddRange([recovering, visible]);
        var saver = new Todos.Application.Persistence.DebouncedAppStateSaver(new ImmediateRepository());
        using var controller = new Todos.Presentation.AppController(state, saver, null,
            new Todos.Application.AppLaunchOptions(false, false, directory.Path, null, null, false, 350),
            new InertStartup(), new Todos.Infrastructure.Logging.LocalLogService(new Todos.Infrastructure.Persistence.AppDataPaths(directory.Path)));
        typeof(Todos.Presentation.AppController).GetField("_displayService", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(controller, new ReviewDisplayService());
        var manager = (Todos.Presentation.Windows.TodoWindowManager)typeof(Todos.Presentation.AppController)
            .GetField("_windowManager", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(controller)!;
        var outsideClick = typeof(Todos.Presentation.AppController).GetMethod("OnOutsideClick", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var show = typeof(Todos.Presentation.AppController).GetMethod("ShowHidden", BindingFlags.Instance | BindingFlags.NonPublic)!;
        manager.Show(visible); Pump();
        var applyClick = (Action)outsideClick.Invoke(controller, [new Point(-10000, -10000)])!;
        show.Invoke(controller, [recovering.Id]);
        applyClick();
        Assert.True(recovering.IsVisible, "The same click must not hide a just-recovered window.");
        Assert.False(visible.IsVisible);
        Assert.Equal(visible.Id, state.LauncherState.LastMinimizedWindowId);
        var recoveringWindow = manager.GetWindow(recovering.Id)!;
        ((Action)outsideClick.Invoke(controller, [PhysicalCenter(recoveringWindow)])!)();
        Assert.True(recovering.IsVisible, "An internal click must keep the list visible.");
        var recoveringVm = (TodoWindowViewModel)recoveringWindow.DataContext;
        var noteTask = recoveringVm.PendingTasks.Single(); noteTask.NotesMarkdown = "# Nota guardada";
        saver.FlushAsync().GetAwaiter().GetResult(); Pump();
        Assert.Equal("Guardado", noteTask.NotesSaveStatus);
        typeof(TodoWindow).GetMethod("OpenNotes", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(recoveringWindow, [noteTask]); Pump();
        var notes = (Dictionary<Guid, TaskNotesWindow>)typeof(TodoWindow).GetField("_notesWindows", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(recoveringWindow)!;
        var note = notes[noteTask.Id];
        ((Action)outsideClick.Invoke(controller, [PhysicalCenter(note)])!)();
        Assert.True(recovering.IsVisible, "Working in notes must not minimize the owning list.");
        noteTask.NotesMarkdown = "# Nueva edición";
        saver.Schedule(state, TimeSpan.FromSeconds(30));
        typeof(Todos.Presentation.AppController).GetMethod("OnSaveCompleted", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(controller,
            [new AppState { TodoWindows = [new TodoWindowState { Id = recovering.Id, Tasks = [new TodoTaskItem { Id = noteTask.Id, Text = noteTask.Text, NotesMarkdown = "# Nota guardada" }] }] }, null]);
        Pump();
        Assert.Equal("Guardando…", noteTask.NotesSaveStatus);
        saver.FlushAsync().GetAwaiter().GetResult(); Pump(); Assert.Equal("Guardado", noteTask.NotesSaveStatus);
        note.Close();
        state.Settings.AutoMinimizeWindows = false;
        ((Action)outsideClick.Invoke(controller, [new Point(-10000, -10000)])!)();
        Assert.True(recovering.IsVisible);
        state.Settings.AutoMinimizeWindows = true;
        ((Action)outsideClick.Invoke(controller, [new Point(-10000, -10000)])!)();
        Assert.False(recovering.IsVisible);
        Assert.Equal(recovering.Id, state.LauncherState.LastMinimizedWindowId);
        typeof(Todos.Presentation.AppController).GetMethod("RequestDelete", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(controller, [recovering]);
        Assert.False(state.TodoWindows.Contains(recovering));
        Assert.True(state.TrashedWindows.Contains(recovering));
        Assert.Equal<Guid?>(null, state.LauncherState.LastMinimizedWindowId);
        Assert.True(manager.GetWindow(recovering.Id) is null);
        var showBin = typeof(Todos.Presentation.AppController).GetMethod("ShowRecycleBin", BindingFlags.Instance | BindingFlags.NonPublic)!;
        showBin.Invoke(controller, null); Pump();
        var bin = (RecycleBinWindow)typeof(Todos.Presentation.AppController).GetField("_recycleBinWindow", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(controller)!;
        Assert.True(bin.IsVisible, "The trash action must open its window.");
        showBin.Invoke(controller, null);
        Assert.True(ReferenceEquals(bin, typeof(Todos.Presentation.AppController).GetField("_recycleBinWindow", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(controller)));
        var binVm = (RecycleBinViewModel)bin.DataContext;
        Assert.Equal(recovering.Id, binVm.Lists.Single().Id);
        binVm.Lists.Single().RestoreCommand.Execute(null);
        Assert.True(state.TodoWindows.Contains(recovering));
        Assert.Equal(0, state.TrashedWindows.Count);
        Assert.True(manager.GetWindow(recovering.Id)!.IsVisible);
    }

    private sealed class ImmediateRepository : Todos.Application.Persistence.IAppStateRepository
    {
        public Task<Todos.Application.Persistence.AppStateLoadResult> LoadAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task SaveAsync(AppState state, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class InertStartup : Todos.Application.Windows.IStartupRegistrationService
    {
        public Todos.Application.Windows.StartupRegistrationStatus GetStatus() => new(false, false, null);
        public void Enable() { }
        public void Disable() { }
    }

    private static void VerifyLauncher(string? output)
    {
        var settingsChanged = false;
        Guid? deletedFromMenu = null;
        LauncherWindow? launcher = null;
        var launcherVm = new LauncherViewModel(false, () => { }, () => { }, () => { }, _ => { }, id => deletedFromMenu = id, _ => true,
            () => Task.CompletedTask, false, enabled => settingsChanged = enabled,
            hideLauncher: () => launcher?.MinimizeLauncher());
        var states = new[]
        {
            new TodoWindowState { Title = "Proyecto API", ColorTheme = TodoColorTheme.Rose, IsVisible = false },
            new TodoWindowState { Title = "Frontend", ColorTheme = TodoColorTheme.Sky, IsVisible = false },
        };
        launcherVm.RefreshHiddenWindows(states);
        launcherVm.RefreshAssociation(states, states[0].Id);
        launcher = new LauncherWindow(launcherVm) { AllowClose = true, ShowActivated = false };
        PlaceForReview(launcher, 600, 300);
        try
        {
            launcher.Show(); Pump();
            var surface = (Border)launcher.FindName("LauncherSurface");
            Assert.False(Descendants<System.Windows.Controls.Image>(surface).Any(), "Main control must use vectors.");
            Assert.True(Descendants<System.Windows.Shapes.Ellipse>(surface).Any());
            Assert.Equal("Assets/Icons/check.svg", Descendants<Todos.Presentation.Controls.SvgIcon>(surface).Single().Source);
            Assert.Equal((Color)ColorConverter.ConvertFromString(Todos.Presentation.Themes.TodoThemePalette.GetLauncherBackground(states[0].ColorTheme)),
                ((SolidColorBrush)Descendants<System.Windows.Shapes.Ellipse>(surface).Single().Fill).Color);
            Assert.Equal(Colors.White, ((SolidColorBrush)Descendants<Todos.Presentation.Controls.SvgIcon>(surface).Single().Foreground).Color);
            var namePopup = (System.Windows.Controls.Primitives.Popup)launcher.FindName("NamePopup");
            Assert.False(namePopup.IsOpen);
            var hoverTimer = (DispatcherTimer)typeof(LauncherWindow).GetField("_hoverTimer", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(launcher)!;
            Assert.Equal(TimeSpan.FromSeconds(1), hoverTimer.Interval);
            surface.RaiseEvent(new System.Windows.Input.MouseEventArgs(System.Windows.Input.Mouse.PrimaryDevice, 0) { RoutedEvent = UIElement.MouseEnterEvent });
            Pump();
            Assert.True(namePopup.IsOpen);
            typeof(LauncherWindow).GetMethod("BeginLauncherDrag", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(launcher, null);
            Assert.False(namePopup.IsOpen);
            surface.RaiseEvent(new System.Windows.Input.MouseEventArgs(System.Windows.Input.Mouse.PrimaryDevice, 0) { RoutedEvent = UIElement.MouseEnterEvent }); Pump();
            if (output is not null) RenderLauncher((FrameworkElement)namePopup.Child, Path.Combine(output, "launcher-hover-label.png"));
            surface.RaiseEvent(new System.Windows.Input.MouseEventArgs(System.Windows.Input.Mouse.PrimaryDevice, 0) { RoutedEvent = UIElement.MouseLeaveEvent });
            Assert.False(namePopup.IsOpen);
            Assert.True(launcher.ContainsInteraction(PhysicalCenter(surface)));
            if (output is not null) RenderLauncher(launcher.LauncherButtonPreviewContent, Path.Combine(output, "launcher-associated.png"));
            launcherVm.ShowHiddenLists(); Pump();
            var popup = (System.Windows.Controls.Primitives.Popup)launcher.FindName("ButtonsPopup");
            Assert.True(popup.IsOpen);
            var hidden = (ScrollViewer)launcher.FindName("HiddenPanel");
            Assert.True(hidden.IsVisible);
            Assert.Equal(2, Descendants<Button>(hidden).Count());
            Assert.True(launcher.ContainsInteraction(PhysicalCenter(hidden)), "The hook uses physical coordinates, not a WPF round-trip point.");
            if (output is not null) RenderLauncher(launcher.HiddenSubmenuPreviewContent, Path.Combine(output, "launcher-hidden.png"));
            var hiddenButton = Descendants<Button>(hidden).First();
            var hiddenRow = (Grid)VisualTreeHelper.GetParent(hiddenButton);
            hiddenRow.RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(System.Windows.Input.Mouse.PrimaryDevice, 0, System.Windows.Input.MouseButton.Right)
                { RoutedEvent = UIElement.PreviewMouseRightButtonDownEvent }); Pump();
            var listMenu = (System.Windows.Controls.Primitives.Popup)launcher.FindName("ListMenuPopup");
            Assert.True(listMenu.IsOpen, "Right click on a hidden list row must open its menu.");
            Assert.True(ReferenceEquals(hiddenButton, listMenu.PlacementTarget), "Anchor the menu to the circle, not the empty full-width row.");
            Assert.True(launcher.ContainsInteraction(PhysicalCenter((FrameworkElement)listMenu.Child)));
            if (output is not null) RenderLauncher((FrameworkElement)listMenu.Child, Path.Combine(output, "launcher-list-menu.png"));
            ((Button)launcher.FindName("MoveListToTrashButton")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal<Guid?>(((HiddenTodoViewModel)hiddenButton.DataContext).Id, deletedFromMenu);
            Assert.False(listMenu.IsOpen);

            surface.RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(System.Windows.Input.Mouse.PrimaryDevice, 0, System.Windows.Input.MouseButton.Right)
            { RoutedEvent = UIElement.PreviewMouseRightButtonDownEvent });
            Pump();
            Assert.True(launcherVm.IsMenuOpen);
            Assert.False(hidden.IsVisible);
            var actions = (StackPanel)launcher.FindName("ActionsPanel");
            Assert.Equal(5, Descendants<Button>(actions).Count());
            launcherVm.ShowHiddenLists();
            Assert.True(launcherVm.IsMenuOpen);
            if (output is not null) RenderLauncher(launcher.MenuPreviewContent, Path.Combine(output, "launcher-actions.png"));
            Assert.SequenceEqual(new[] { "Salir", "Minimizar botón", "Configuración", "Papelera", "Nueva lista" },
                Descendants<Button>(actions).Select(button => button.ToolTip?.ToString() ?? string.Empty));
            Descendants<Button>(actions).Single(button => Equals(button.ToolTip, "Minimizar botón")).Command.Execute(null); Pump();
            Assert.False(launcher.IsVisible);
            Assert.False(popup.IsOpen);
            Assert.False(namePopup.IsOpen);
            Assert.False(listMenu.IsOpen);
            Assert.False(hoverTimer.IsEnabled);
            launcher.KeepAboveTodoWindows();
            Assert.False(launcher.IsVisible);
            Assert.Equal<Guid?>(states[0].Id, launcherVm.AssociatedWindowId);
            launcher.BringLauncherToFront(); Pump();
            Assert.True(launcher.IsVisible);
            Assert.False(launcherVm.IsPanelOpen);
            launcherVm.ToggleMenu(); Pump();
            Descendants<Button>(actions).Single(button => Equals(button.ToolTip, "Configuración"))
                .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Pump();
            var settings = (System.Windows.Controls.Primitives.Popup)launcher.FindName("SettingsPopup");
            Assert.True(settings.IsOpen);
            Assert.False(popup.IsOpen, "Opening settings must hide secondary actions.");
            var checks = Descendants<CheckBox>(settings.Child).ToArray();
            Assert.Equal(2, checks.Length);
            Assert.True(checks.All(check => check.VerticalContentAlignment == VerticalAlignment.Center));
            foreach (var check in checks)
            {
                check.ApplyTemplate();
                var content = Descendants<ContentPresenter>(check).Single();
                var icon = Descendants<Todos.Presentation.Controls.SvgIcon>(check).Single();
                var contentCenter = content.TranslatePoint(new Point(0, content.ActualHeight / 2), check).Y;
                var iconCenter = icon.TranslatePoint(new Point(0, icon.ActualHeight / 2), check).Y;
                Assert.True(Math.Abs(contentCenter - iconCenter) <= 1, "Settings labels must align vertically with their checkboxes.");
            }
            checks[1].IsChecked = true;
            Assert.True(settingsChanged);
            Assert.Equal("Assets/Icons/checkbox-checked.svg", Descendants<Todos.Presentation.Controls.SvgIcon>(checks[1]).Single().Source);
            Assert.True(launcher.ContainsInteraction(PhysicalCenter((FrameworkElement)settings.Child)));
            if (output is not null) RenderLauncher((FrameworkElement)settings.Child, Path.Combine(output, "launcher-settings.png"));
            launcher.DismissPanels(); Pump();
            Assert.False(popup.IsOpen);
            Assert.False(settings.IsOpen);
            launcherVm.RefreshAssociation(states, null);
            if (output is not null) RenderLauncher(launcher.LauncherButtonPreviewContent, Path.Combine(output, "launcher-default.png"));
            // A long collection must remain bounded and scrollable, including at the screen edge.
            launcherVm.RefreshHiddenWindows(Enumerable.Range(0, 100).Select(index => new TodoWindowState { Title = $"Lista {index}", IsVisible = false }));
            launcher.Top = ReviewArea.Top + 10;
            launcherVm.ShowHiddenLists(); Pump();
            Assert.True(hidden.ScrollableHeight > 0, $"Hidden scroll height {hidden.ScrollableHeight}, actual {hidden.ActualHeight}, max {hidden.MaxHeight}");
            var screen = System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position).WorkingArea;
            screen = ReviewScreen.WorkingArea;
            var popupOrigin = ((FrameworkElement)popup.Child).PointToScreen(new Point());
            Assert.True(popupOrigin.Y >= screen.Top, $"Popup origin {popupOrigin}");
            Assert.True(hidden.ActualHeight < screen.Height, $"Panel {hidden.ActualHeight}, screen {screen.Height}");
            var bar = Descendants<System.Windows.Controls.Primitives.ScrollBar>(hidden).Single(item => item.Orientation == Orientation.Vertical);
            Assert.True(bar.ActualWidth <= 10, $"Scrollbar width {bar.ActualWidth}, width {bar.Width}, min {bar.MinWidth}, style {bar.Style == System.Windows.Application.Current.FindResource("TodosSlimScrollBar")}");
            hidden.ScrollToEnd(); Pump();
            Assert.True(hidden.VerticalOffset > 0, "The slim scrollbar must still scroll the collection.");
            if (output is not null) RenderLauncher(launcher.HiddenSubmenuPreviewContent, Path.Combine(output, "launcher-many-hidden.png"));
            launcher.DismissPanels(); Pump();
            launcher.Left = ReviewArea.Left;
            launcherVm.IsMenuOpen = true; Pump();
            Assert.Equal(FlowDirection.RightToLeft, actions.FlowDirection);
            if (output is not null) RenderLauncher(launcher.MenuPreviewContent, Path.Combine(output, "launcher-actions-labels-right.png"));
        }
        finally { launcher.Close(); }
    }

    private static void VerifyRecycleBin(string? output)
    {
        var state = new AppState();
        var first = new TodoWindowState { Title = "Proyecto API", ColorTheme = TodoColorTheme.Sky };
        var second = new TodoWindowState { Title = "Tinta y libro", ColorTheme = TodoColorTheme.Rose };
        TodoListOperations.AddTask(first, "Conservar tarea", DateTimeOffset.UtcNow);
        state.TodoWindows.AddRange([first, second]);
        RecycleBinOperations.MoveToTrash(state, first.Id, DateTimeOffset.UtcNow);
        RecycleBinOperations.MoveToTrash(state, second.Id, DateTimeOffset.UtcNow);
        RecycleBinViewModel? vm = null;
        vm = new RecycleBinViewModel(id => { RecycleBinOperations.Restore(state, id); vm!.Refresh(state.TrashedWindows); },
            id => { RecycleBinOperations.DeletePermanently(state, id); vm!.Refresh(state.TrashedWindows); });
        vm.Refresh(state.TrashedWindows);
        var window = new RecycleBinWindow(vm) { ShowActivated = false };
        PlaceForReview(window, 80, 50);
        try
        {
            PlaceForReview(window); window.Show(); Pump();
            var surface = (FrameworkElement)window.FindName("BinSurface");
            if (output is not null) RenderLauncher(surface, Path.Combine(output, "recycle-bin.png"));
            vm.Lists.First().DeleteCommand.Execute(null); Pump();
            Assert.True(((FrameworkElement)window.FindName("DeleteConfirmation")).IsVisible);
            Assert.Equal(2, state.TrashedWindows.Count);
            if (output is not null) RenderLauncher(surface, Path.Combine(output, "recycle-bin-confirm.png"));
            vm.CancelDeleteCommand.Execute(null);
            Assert.Equal(2, state.TrashedWindows.Count);
            vm.Lists.Single(item => item.Id == first.Id).RestoreCommand.Execute(null); Pump();
            Assert.True(state.TodoWindows[0].IsVisible);
            Assert.Equal("Conservar tarea", state.TodoWindows[0].Tasks[0].Text);
            vm.Lists.First().DeleteCommand.Execute(null);
            vm.ConfirmDeleteCommand.Execute(null); Pump();
            Assert.True(vm.IsEmpty);
            Assert.False(((FrameworkElement)window.FindName("DeleteConfirmation")).IsVisible);
            if (output is not null) RenderLauncher(surface, Path.Combine(output, "recycle-bin-empty.png"));
            vm.Refresh(Enumerable.Range(0, 20).Select(index => new TodoWindowState { Title = $"Lista {index}", TrashedAt = DateTimeOffset.UtcNow })); Pump();
            var scroll = (ScrollViewer)window.FindName("BinScroll");
            Assert.True(scroll.ScrollableHeight > 0);
            scroll.ScrollToEnd(); Pump();
            Assert.True(scroll.VerticalOffset > 0);
            if (output is not null) RenderLauncher(surface, Path.Combine(output, "recycle-bin-many.png"));
        }
        finally { window.Close(); }
    }

    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) yield return match;
            foreach (var descendant in Descendants<T>(child)) yield return descendant;
        }
    }

    private static void Pump()
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        timer.Tick += (_, _) => { timer.Stop(); frame.Continue = false; };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }

    private static void RaiseDrag(Border target, TodoTaskViewModel source, RoutedEvent routedEvent, double y)
    {
        var args = (DragEventArgs)Activator.CreateInstance(typeof(DragEventArgs),
            BindingFlags.Instance | BindingFlags.NonPublic, null,
            [new DataObject(typeof(TodoTaskViewModel), source), DragDropKeyStates.LeftMouseButton,
                DragDropEffects.Move, target, new Point(12, y)], null)!;
        args.RoutedEvent = routedEvent;
        target.RaiseEvent(args);
    }

    private static void RenderLauncher(FrameworkElement view, string path)
    {
        view.UpdateLayout();
        var dpi = VisualTreeHelper.GetDpi(view);
        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(view.ActualWidth * dpi.DpiScaleX),
            (int)Math.Ceiling(view.ActualHeight * dpi.DpiScaleY), 96 * dpi.DpiScaleX, 96 * dpi.DpiScaleY, PixelFormats.Pbgra32);
        bitmap.Render(view);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var file = File.Create(path);
        encoder.Save(file);
    }

    private static void Render(FrameworkElement view, string path)
    {
        view.UpdateLayout();
        var bitmap = new RenderTargetBitmap((int)view.ActualWidth, (int)view.ActualHeight, 96, 96, PixelFormats.Pbgra32);
        var visual = new DrawingVisual();
        using (var drawing = visual.RenderOpen())
            drawing.DrawRectangle(new VisualBrush(view)
            {
                Stretch = Stretch.Fill,
            }, null, new Rect(0, 0, view.ActualWidth, view.ActualHeight));
        bitmap.Render(visual);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var output = File.Create(path);
        encoder.Save(output);
    }
}
