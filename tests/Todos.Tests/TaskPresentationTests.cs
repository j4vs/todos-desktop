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
                window.Show();
                Pump();
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
            }
            catch (Exception exception) { failure = exception; }
            finally { window?.Close(); app?.Shutdown(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null) throw failure;
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
