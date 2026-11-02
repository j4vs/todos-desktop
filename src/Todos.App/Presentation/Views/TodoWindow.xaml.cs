using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Point = System.Windows.Point;
using DragDropEffects = System.Windows.DragDropEffects;
using Todos.Presentation.Themes;
using Todos.Domain;
using Todos.Domain.Models;
using Todos.Presentation.ViewModels;
using Todos.Presentation.Windows;

namespace Todos.Presentation.Views;

public partial class TodoWindow : System.Windows.Window
{
    private TodoTaskViewModel? _draggedTask;
    private System.Windows.Point _dragStart;
    private Border? _activeDropRow;
    private Grid? _dragPreview;
    private object _sourceOpacity = DependencyProperty.UnsetValue;
    private System.Windows.Point _dragGrabOffset;
    private System.Windows.Point? _headerDragStart;
    private Border? _sourceRow;
    private Border? _groupDropTarget;
    private System.Windows.Media.Brush? _groupOriginalBackground;
    private DateTime _lastAutoScroll;
    private TodoTaskViewModel? _groupMenuTask;
    private FrameworkElement? _groupMenuAnchor;
    private TodoGroupViewModel? _groupPendingDeletion;
    private TodoTaskViewModel? _draggedSubtask;
    private System.Windows.Point _subtaskDragStart;
    private readonly Dictionary<Guid, TaskNotesWindow> _notesWindows = [];

    public TodoWindow(TodoWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        UpdateThemeResources();
        viewModel.PropertyChanged += OnThemePropertyChanged;
        Closing += OnClosing;
        Closed += (_, _) =>
        {
            CloseNotes();
            viewModel.PendingTasks.CollectionChanged -= OnTasksChanged;
            viewModel.CompletedTasks.CollectionChanged -= OnTasksChanged;
            viewModel.PropertyChanged -= OnThemePropertyChanged; viewModel.Dispose();
        };
        viewModel.PendingTasks.CollectionChanged += OnTasksChanged;
        viewModel.CompletedTasks.CollectionChanged += OnTasksChanged;
        Loaded += OnLoaded;
        GiveFeedback += OnDragGiveFeedback;
        DeleteGroupPopup.Closed += (_, _) => _groupPendingDeletion = null;
        IsVisibleChanged += (_, _) =>
        {
            if (!IsVisible) { CloseNotes(); GroupMenuPopup.IsOpen = false; DeleteGroupPopup.IsOpen = false; }
        };
    }

    private void OnThemePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TodoWindowViewModel.ColorTheme))
        {
            UpdateThemeResources();
            foreach (var note in _notesWindows.Values) note.ApplyTheme(ViewModel.ColorTheme);
        }
    }

    private void UpdateThemeResources() => WindowThemeResources.Apply(Resources, ViewModel.ColorTheme);

    public bool AllowClose { get; set; }

    public bool ContainsInteraction(System.Windows.Point screenPoint) =>
        _notesWindows.Values.Any(note => note.IsFileDialogOpen || ScreenInteraction.Contains(note, screenPoint)) ||
        ScreenInteraction.Contains(this, screenPoint) ||
        (PalettePopup.IsOpen && ScreenInteraction.Contains(PalettePopup.Child as FrameworkElement, screenPoint)) ||
        (GroupMenuPopup.IsOpen && ScreenInteraction.Contains(GroupMenuPopup.Child as FrameworkElement, screenPoint)) ||
        (DeleteGroupPopup.IsOpen && ScreenInteraction.Contains(DeleteGroupPopup.Child as FrameworkElement, screenPoint));

    private TodoWindowViewModel ViewModel => (TodoWindowViewModel)DataContext;

    private void CloseNotes()
    {
        foreach (var note in _notesWindows.Values.ToArray()) note.Close();
    }
    private void OnTasksChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            var ids = ViewModel.PendingTasks.Concat(ViewModel.CompletedTasks).SelectMany(root => new[] { root }.Concat(root.Subtasks))
                .Select(task => task.Id).ToHashSet();
            foreach (var pair in _notesWindows.ToArray()) if (!ids.Contains(pair.Key)) pair.Value.Close();
        }));
    }
    private void OnOpenNotes(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: TodoTaskViewModel task }) OpenNotes(task);
        e.Handled = true;
    }
    private void OnNotesMenu(object sender, RoutedEventArgs e)
    {
        var task = _groupMenuTask; _groupMenuAnchor = null; GroupMenuPopup.IsOpen = false;
        if (task is not null) OpenNotes(task);
    }
    private void OpenNotes(TodoTaskViewModel task)
    {
        if (_notesWindows.TryGetValue(task.Id, out var existing)) { existing.Activate(); return; }
        var note = new TaskNotesWindow(task, ViewModel.ColorTheme) { Owner = this };
        var area = new Todos.Infrastructure.Windows.WindowsDisplayService().GetBestWorkingArea(Left, Top);
        note.Width = Math.Min(note.Width, area.Width); note.Height = Math.Min(note.Height, area.Height);
        note.Left = Math.Clamp(Left + 30, area.Left, Math.Max(area.Left, area.Right - note.Width));
        note.Top = Math.Clamp(Top + 30, area.Top, Math.Max(area.Top, area.Bottom - note.Height));
        _notesWindows[task.Id] = note; note.Closed += (_, _) => _notesWindows.Remove(task.Id);
        note.Show();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!AnimationSettings.AreAnimationsEnabled)
        {
            return;
        }

        var duration = new System.Windows.Duration(AnimationSettings.WindowEntranceDuration);
        var easing = new QuadraticEase { EasingMode = EasingMode.EaseOut };
        RootContainer.Opacity = 0;
        EntranceScale.ScaleX = 0.97;
        EntranceScale.ScaleY = 0.97;
        RootContainer.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, duration) { EasingFunction = easing });
        EntranceScale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.97, 1, duration) { EasingFunction = easing });
        EntranceScale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.97, 1, duration) { EasingFunction = easing });
    }

    private void OnHeaderMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!WindowHeaderInputPolicy.CanStartDrag(e.OriginalSource as DependencyObject, HeaderArea, TitleEditor)) return;
        if (e.ClickCount == 2 && IsWithinTitle(e.OriginalSource as DependencyObject))
        {
            BeginTitleEdit();
            e.Handled = true;
            return;
        }
        _headerDragStart = e.GetPosition(this);
        HeaderArea.CaptureMouse();
        e.Handled = true;
    }

    private bool IsWithinTitle(DependencyObject? source)
    {
        for (var current = source; current is not null && current != HeaderArea; current = VisualTreeHelper.GetParent(current))
            if (current == TitleEditor) return true;
        return false;
    }

    private void BeginTitleEdit()
    {
        TitleEditor.IsReadOnly = false;
        TitleEditor.Cursor = System.Windows.Input.Cursors.IBeam;
        TitleEditor.Focus();
        TitleEditor.SelectAll();
    }

    private void OnHeaderMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_headerDragStart is not { } start || e.LeftButton != MouseButtonState.Pressed) return;
        var point = e.GetPosition(this);
        if (Math.Abs(point.X - start.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(point.Y - start.Y) < SystemParameters.MinimumVerticalDragDistance) return;
        _headerDragStart = null;
        HeaderArea.ReleaseMouseCapture();
        try
        {
            DragMove();
            ViewModel.CommitBounds(Left, Top, ActualWidth, ActualHeight);
        }
        catch (InvalidOperationException)
        {
            // Windows can cancel a drag when the pointer is released immediately.
        }
        e.Handled = true;
    }

    private void OnHeaderMouseUp(object sender, MouseButtonEventArgs e)
    {
        _headerDragStart = null;
        if (HeaderArea.IsMouseCaptured) HeaderArea.ReleaseMouseCapture();
    }

    private void OnHeaderLostCapture(object sender, System.Windows.Input.MouseEventArgs e) => _headerDragStart = null;

    private void OnTitleKeyDown(object sender, KeyEventArgs e)
    {
        if (TitleEditor.IsReadOnly && e.Key is Key.Enter or Key.F2)
        {
            BeginTitleEdit();
            e.Handled = true;
            return;
        }
        if (e.Key == Key.Enter)
        {
            ViewModel.CommitTitle();
            Keyboard.ClearFocus();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            TitleEditor.Text = ViewModel.Title;
            Keyboard.ClearFocus();
            e.Handled = true;
        }
    }

    private void OnTitleLostFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        ViewModel.CommitTitle();
        TitleEditor.IsReadOnly = true;
        TitleEditor.Cursor = System.Windows.Input.Cursors.SizeAll;
    }

    private void OnNewTaskKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || !ViewModel.AddTaskCommand.CanExecute(null))
        {
            return;
        }

        ViewModel.AddTaskCommand.Execute(null);
        NewTaskEditor.Focus();
        e.Handled = true;
    }

    private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
        {
            NewTaskEditor.Focus();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && PalettePopup.IsOpen)
        {
            PalettePopup.IsOpen = false;
            PaletteButton.Focus();
            e.Handled = true;
        }
    }

    private void OnTaskRowKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: TodoTaskViewModel task })
        {
            return;
        }

        if (e.OriginalSource is DependencyObject origin &&
            FindAncestor<Border>(origin, row => row.Name is "TaskRowContainer" or "SubtaskRow") is { } owner && !ReferenceEquals(owner, sender)) return;

        var tasks = task.ParentId is { } parentId
            ? ViewModel.PendingTasks.Concat(ViewModel.CompletedTasks).Single(root => root.Id == parentId).Subtasks
            : ViewModel.Groups.Single(group => group.Id == task.GroupId).Tasks;
        var currentIndex = tasks.IndexOf(task);
        if (Keyboard.Modifiers == ModifierKeys.Alt && e.Key is Key.Up or Key.Down)
        {
            var offset = e.Key == Key.Up ? -1 : 1;
            var destinationIndex = Math.Clamp(currentIndex + offset, 0, tasks.Count - 1);
            if (task.ParentId is not null) ViewModel.MoveSubtask(task.Id, destinationIndex);
            else ViewModel.MovePendingTask(task.Id, destinationIndex);
            e.Handled = true;
        }
        else if (e.Key == Key.Delete && ReferenceEquals(Keyboard.FocusedElement, sender))
        {
            task.DeleteCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void OnTaskRowMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2 || IsInteractiveSource(e.OriginalSource as DependencyObject) ||
            sender is not FrameworkElement { DataContext: TodoTaskViewModel task })
        {
            return;
        }

        task.BeginEditCommand.Execute(null);
        Dispatcher.BeginInvoke(() => FindDescendant<TextBox>((DependencyObject)sender)?.Focus());
        e.Handled = true;
    }

    private void OnTaskEditKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox { DataContext: TodoTaskViewModel task })
        {
            return;
        }

        if (e.Key == Key.Enter)
        {
            task.SaveEdit();
            Keyboard.ClearFocus();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            task.CancelEdit();
            Keyboard.ClearFocus();
            e.Handled = true;
        }
    }

    private void OnTaskEditLostFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBox { DataContext: TodoTaskViewModel task } && task.IsEditing && !task.SaveEdit())
        {
            task.CancelEdit();
        }
    }

    private void OnGripMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: TodoTaskViewModel task })
        {
            _draggedTask = task;
            _dragStart = e.GetPosition(PendingItems);
            var row = FindAncestor<Border>((DependencyObject)sender, element => element.Name == "TaskRowContainer");
            _dragGrabOffset = row is null ? new System.Windows.Point() : e.GetPosition(row);
        }
    }

    private void OnGripMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_draggedTask is null || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var current = e.GetPosition(PendingItems);
        if (Math.Abs(current.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(current.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        try
        {
            BeginRowDrag(FindAncestor<Border>((DependencyObject)sender, row => row.Name == "TaskRowContainer"), _draggedTask);
            System.Windows.DragDrop.DoDragDrop((DependencyObject)sender, _draggedTask, System.Windows.DragDropEffects.Move);
        }
        finally
        {
            EndRowDrag();
            _draggedTask = null;
        }
    }

    private sealed record TaskDropTarget(TodoGroupViewModel Group, int Index, Border GroupBorder, Border? Row, bool After);

    // Resolve from stable row bounds for the entire task surface. Margins between
    // rows are insertion slots too, never an accidental "append to group" action.
    private TaskDropTarget? ResolveTaskDrop(TodoTaskViewModel source, System.Windows.Point pointer)
    {
        var groups = GroupBorders(PendingItems).Where(border => border.IsVisible).ToArray();
        if (groups.Length == 0) return null;
        var groupBorder = groups.MinBy(border =>
        {
            var top = border.TranslatePoint(new System.Windows.Point(), PendingItems).Y;
            return Math.Max(top - pointer.Y, Math.Max(0, pointer.Y - top - border.ActualHeight));
        })!;
        var group = (TodoGroupViewModel)groupBorder.DataContext;
        var rows = TaskRows(groupBorder).Where(row => row.IsVisible && ((TodoTaskViewModel)row.DataContext).Id != source.Id).ToArray();
        if (!group.IsExpanded || rows.Length == 0)
            return new TaskDropTarget(group, group.Tasks.Count(task => task.Id != source.Id), groupBorder, null, true);
        var targetRow = rows.FirstOrDefault(row => pointer.Y < row.TranslatePoint(new System.Windows.Point(), PendingItems).Y + row.ActualHeight / 2);
        var after = targetRow is null;
        targetRow ??= rows[^1];
        var target = (TodoTaskViewModel)targetRow.DataContext;
        var index = group.Tasks.IndexOf(target) + (after ? 1 : 0);
        var sourceIndex = group.Tasks.IndexOf(source);
        if (sourceIndex >= 0 && sourceIndex < index) index--;
        return new TaskDropTarget(group, index, groupBorder, targetRow, after);
    }

    private static IEnumerable<Border> GroupBorders(DependencyObject root)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is Border { Name: "TaskGroupContainer" } border) yield return border;
            else foreach (var group in GroupBorders(child)) yield return group;
        }
    }

    private void OnTasksDragOver(object sender, System.Windows.DragEventArgs e)
    {
        e.Handled = true;
        if (e.Data.GetData(typeof(TodoTaskViewModel)) is not TodoTaskViewModel source || _draggedTask != source)
        {
            ClearDropCue(); ClearGroupCue();
            e.Effects = System.Windows.DragDropEffects.None;
            return;
        }
        AutoScroll(e);
        TasksScrollViewer.UpdateLayout();
        var target = ResolveTaskDrop(source, e.GetPosition(PendingItems));
        if (target is null) { e.Effects = System.Windows.DragDropEffects.None; return; }
        if (target.Row is { } row)
        {
            ClearGroupCue();
            ShowDropCue(row, target.After);
        }
        else
        {
            ClearDropCue();
            if (_groupDropTarget != target.GroupBorder)
            {
                ClearGroupCue();
                _groupDropTarget = target.GroupBorder;
                _groupOriginalBackground = target.GroupBorder.Background;
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(TodoThemePalette.Get(ViewModel.ColorTheme).Accent);
                color.A = 35;
                target.GroupBorder.Background = new SolidColorBrush(color);
            }
        }
        e.Effects = System.Windows.DragDropEffects.Move;
    }

    private void OnTasksDragLeave(object sender, System.Windows.DragEventArgs e)
    {
        var point = e.GetPosition(TasksScrollViewer);
        if (point.X < 0 || point.Y < 0 || point.X > TasksScrollViewer.ActualWidth || point.Y > TasksScrollViewer.ActualHeight)
        { ClearDropCue(); ClearGroupCue(); }
    }

    private void OnTasksDrop(object sender, System.Windows.DragEventArgs e)
    {
        e.Handled = true;
        if (e.Data.GetData(typeof(TodoTaskViewModel)) is not TodoTaskViewModel source || _draggedTask != source) return;
        var target = ResolveTaskDrop(source, e.GetPosition(PendingItems));
        ClearDropCue(); ClearGroupCue();
        if (target is not null && (source.GroupId != target.Group.Id || target.Group.Tasks.IndexOf(source) != target.Index))
            ApplyAnimatedMove(() => ViewModel.MoveTaskToGroup(source.Id, target.Group.Id, target.Index));
        _draggedTask = null;
    }

    private void ShowDropCue(Border row, bool insertAfterTarget)
    {
        var before = !insertAfterTarget;
        if (!ReferenceEquals(_activeDropRow, row))
        {
            ClearDropCue();
            _activeDropRow = row;
        }

        var activeIndicator = FindNamedDescendant<Border>(row, before ? "DropBeforeIndicator" : "DropAfterIndicator");
        var inactiveIndicator = FindNamedDescendant<Border>(row, before ? "DropAfterIndicator" : "DropBeforeIndicator");
        AnimateDropIndicator(activeIndicator, visible: true);
        AnimateDropIndicator(inactiveIndicator, visible: false);
    }

    private void ClearDropCue()
    {
        if (_activeDropRow is not null)
        {
            AnimateDropIndicator(FindNamedDescendant<Border>(_activeDropRow, "DropBeforeIndicator"), visible: false);
            AnimateDropIndicator(FindNamedDescendant<Border>(_activeDropRow, "DropAfterIndicator"), visible: false);
        }

        _activeDropRow = null;
    }

    private static void AnimateDropIndicator(Border? indicator, bool visible)
    {
        if (indicator is null) return;
        indicator.BeginAnimation(OpacityProperty, null);
        indicator.Opacity = visible ? 1 : 0;
        var scale = TaskDragVisualPolicy.EnsureMutableScaleTransform(indicator);
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        scale.ScaleX = 1;
    }

    private void OnShowGroupEditor(object sender, RoutedEventArgs e)
    {
        GroupEditorPanel.Visibility = GroupEditorPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        if (GroupEditorPanel.Visibility == Visibility.Visible) GroupNameEditor.Focus();
    }

    private void OnCreateGroup(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.AddGroupCommand.CanExecute(null)) return;
        ViewModel.AddGroupCommand.Execute(null);
        if (string.IsNullOrEmpty(ViewModel.GroupError))
        {
            GroupMenuPopup.IsOpen = false;
            NewTaskEditor.Focus();
        }
    }

    private void OnNewGroupKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) { OnCreateGroup(sender, e); e.Handled = true; }
        if (e.Key == Key.Escape) { GroupMenuPopup.IsOpen = false; NewTaskEditor.Focus(); e.Handled = true; }
    }

    private void OnGroupNameLostFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: TodoGroupViewModel group }) ViewModel.RenameGroup(group);
    }

    private void OnGroupNameKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) { NewTaskEditor.Focus(); e.Handled = true; }
    }

    private void OnMoveGroupUp(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: TodoGroupViewModel group }) ViewModel.MoveGroup(group, -1);
    }

    private void OnMoveGroupDown(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: TodoGroupViewModel group }) ViewModel.MoveGroup(group, 1);
    }

    private void OnRenameGroup(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element) return;
        DependencyObject? parent = element;
        while (parent is not null && parent is not Grid) parent = VisualTreeHelper.GetParent(parent);
        if (parent is Grid grid && grid.Children.OfType<TextBox>().FirstOrDefault() is { } editor)
        {
            Activate();
            editor.Focus();
            editor.SelectAll();
        }
    }

    private void OnRemoveGroup(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: TodoGroupViewModel group } anchor || group.IsGeneral) return;
        GroupMenuPopup.IsOpen = false;
        _groupPendingDeletion = group;
        DeleteGroupMessage.Text = $"¿Eliminar “{group.Name}”? Sus tareas pasarán a General y conservarán su estado.";
        DeleteGroupPopup.PlacementTarget = anchor;
        DeleteGroupPopup.IsOpen = true;
        Dispatcher.BeginInvoke(() => CancelDeleteGroupButton.Focus());
    }

    private void OnToggleGroup(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: TodoGroupViewModel group }) group.IsExpanded = !group.IsExpanded;
    }

    private void OnCancelDeleteGroup(object sender, RoutedEventArgs e)
    {
        _groupPendingDeletion = null;
        DeleteGroupPopup.IsOpen = false;
        DeleteGroupPopup.PlacementTarget?.Focus();
    }

    private void OnConfirmDeleteGroup(object sender, RoutedEventArgs e)
    {
        var group = _groupPendingDeletion;
        _groupPendingDeletion = null;
        DeleteGroupPopup.IsOpen = false;
        if (group is not null) ViewModel.RemoveGroup(group);
        NewTaskEditor.Focus();
    }

    private void OnDeleteGroupKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { OnCancelDeleteGroup(sender, e); e.Handled = true; }
    }

    private void OnSelectCaptureGroup(object sender, RoutedEventArgs e)
    {
        if (GroupMenuPopup.IsOpen && _groupMenuTask is null) { GroupMenuPopup.IsOpen = false; return; }
        OpenGroupMenu(NewTaskGroupButton, null);
    }

    private void OnChooseMenuGroup(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: TodoGroupViewModel group }) return;
        var task = _groupMenuTask;
        GroupMenuPopup.IsOpen = false;
        if (task is null) ViewModel.SelectedGroup = group;
        else if (task.GroupId != group.Id) ViewModel.MoveTaskToGroup(task.Id, group.Id);
        if (task is null) NewTaskEditor.Focus();
    }

    private void OnGroupMenuClosed(object? sender, EventArgs e)
    {
        // A previous popup can finish closing after the next menu has opened.
        if (GroupMenuPopup.IsOpen) return;
        ClearContextTargets();
        _groupMenuTask = null;
        GroupEditorPanel.Visibility = Visibility.Collapsed;
        _groupMenuAnchor?.Focus();
    }

    private void ClearContextTargets()
    {
        if (_groupMenuTask is not null) _groupMenuTask.IsContextTarget = false;
        foreach (var task in ViewModel.PendingTasks.Concat(ViewModel.CompletedTasks)
                     .SelectMany(root => new[] { root }.Concat(root.Subtasks)))
            task.IsContextTarget = false;
    }

    private void OnGroupMenuKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { GroupMenuPopup.IsOpen = false; e.Handled = true; }
        else if (e.Key is Key.Down or Key.Up && Keyboard.FocusedElement is Button button)
        {
            button.MoveFocus(new TraversalRequest(e.Key == Key.Down ? FocusNavigationDirection.Next : FocusNavigationDirection.Previous));
            e.Handled = true;
        }
    }

    private void OnTaskContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (sender is not Border { DataContext: TodoTaskViewModel task } row) return;
        var pointer = e.CursorLeft >= 0 && e.CursorTop >= 0 ? Mouse.GetPosition(row) : new Point(24, 20);
        OpenGroupMenu(row, task, pointer);
        e.Handled = true;
    }

    private void OpenTaskGroupMenu(FrameworkElement anchor, TodoTaskViewModel task)
    {
        OpenGroupMenu(anchor, task);
    }

    private void OpenGroupMenu(FrameworkElement anchor, TodoTaskViewModel? task, Point? pointer = null)
    {
        ClearContextTargets();
        _groupMenuTask = null;
        _groupMenuAnchor = null;
        GroupMenuPopup.IsOpen = false;
        DeleteGroupPopup.IsOpen = false;
        _groupMenuTask = task;
        _groupMenuAnchor = anchor;
        GroupMenuSurface.DataContext = ViewModel;
        GroupMenuCaption.Text = task is null ? "Nueva tarea en…" : task.ParentId is null ? "Opciones de tarea" : "Subtarea";
        CreateGroupMenuButton.Visibility = task is null ? Visibility.Visible : Visibility.Collapsed;
        GroupMenuItems.Visibility = task?.ParentId is not null ? Visibility.Collapsed : Visibility.Visible;
        GroupMenuItems.Tag = task is not null;
        MoveGroupCaption.Visibility = task is not null && task.ParentId is null ? Visibility.Visible : Visibility.Collapsed;
        AddSubtaskMenuButton.Visibility = task is not null && task.ParentId is null ? Visibility.Visible : Visibility.Collapsed;
        NotesMenuButton.Visibility = task is null ? Visibility.Collapsed : Visibility.Visible;
        NotesMenuButton.Content = task?.NotesMenuLabel;
        EditTaskMenuButton.Visibility = CompleteTaskMenuButton.Visibility = DeleteTaskMenuButton.Visibility = task is null ? Visibility.Collapsed : Visibility.Visible;
        if (task is not null)
        {
            task.IsContextTarget = true;
            EditTaskMenuButton.Content = task.ParentId is null ? "Editar tarea" : "Editar subtarea";
            var noun = task.ParentId is not null ? "subtarea" : task.HasSubtasks ? "tarea y subtareas" : "tarea";
            CompleteTaskMenuButton.Content = (task.IsCompleted ? "Reabrir " : "Completar ") + noun;
            DeleteTaskMenuButton.Content = "Eliminar " + noun;
        }
        GroupMenuPopup.PlacementTarget = anchor;
        GroupMenuPopup.Placement = task is null ? System.Windows.Controls.Primitives.PlacementMode.Left : System.Windows.Controls.Primitives.PlacementMode.RelativePoint;
        GroupMenuPopup.HorizontalOffset = task is null ? -9 : (pointer ?? new Point(24, 20)).X + 8;
        GroupMenuPopup.VerticalOffset = task is null ? 0 : (pointer ?? new Point(24, 20)).Y + 6;
        GroupMenuPopup.IsOpen = true;
        Dispatcher.BeginInvoke(() => GroupMenuSurface.MoveFocus(new TraversalRequest(FocusNavigationDirection.First)));
    }

    private void OnAddSubtaskMenu(object sender, RoutedEventArgs e)
    {
        var task = _groupMenuTask;
        _groupMenuAnchor = null;
        GroupMenuPopup.IsOpen = false;
        if (task is null) return;
        task.BeginSubtaskCapture();
        FocusSubtaskEditor(task.Id);
    }
    private void FocusSubtaskEditor(Guid id) => Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
    {
        UpdateLayout();
        Activate();
        var editor = DescendantsOf<TextBox>(this).FirstOrDefault(box => box.Name == "SubtaskEditor" && box.DataContext is TodoTaskViewModel task && task.Id == id);
        if (editor?.IsVisible == true) { editor.Focus(); Keyboard.Focus(editor); }
    }));
    private void OnNewSubtaskKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox { DataContext: TodoTaskViewModel task }) return;
        if (e.Key == Key.Enter && task.AddSubtaskCommand.CanExecute(null))
        { task.AddSubtaskCommand.Execute(null); FocusSubtaskEditor(task.Id); e.Handled = true; }
        else if (e.Key == Key.Escape) { task.EndSubtaskCapture(); e.Handled = true; }
    }
    private void OnCompleteTaskMenu(object sender, RoutedEventArgs e)
    {
        var task = _groupMenuTask;
        GroupMenuPopup.IsOpen = false;
        if (task is not null) task.IsCompleted = !task.IsCompleted;
    }
    private void OnDeleteTaskMenu(object sender, RoutedEventArgs e)
    {
        var task = _groupMenuTask;
        GroupMenuPopup.IsOpen = false;
        task?.DeleteCommand.Execute(null);
    }
    private void OnEditTaskMenu(object sender, RoutedEventArgs e)
    {
        var task = _groupMenuTask;
        _groupMenuAnchor = null;
        GroupMenuPopup.IsOpen = false;
        if (task is null) return;
        task.BeginEditCommand.Execute(null);
        Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
        {
            UpdateLayout(); Activate();
            DescendantsOf<TextBox>(this).FirstOrDefault(box => ReferenceEquals(box.DataContext, task) && box.IsVisible && !box.IsReadOnly)?.Focus();
        }));
    }
    private static IEnumerable<T> DescendantsOf<T>(DependencyObject root) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) yield return match;
            foreach (var nested in DescendantsOf<T>(child)) yield return nested;
        }
    }
    private sealed record SubtaskDragItem(Guid Id, Guid ParentId);
    private void BeginRowDrag(Border? row, TodoTaskViewModel task)
    {
        _sourceRow = row;
        _sourceOpacity = row?.ReadLocalValue(OpacityProperty) ?? DependencyProperty.UnsetValue;
        ShowDragPreview(task);
        if (row is not null) row.Opacity = 0.3;
    }
    private void EndRowDrag()
    {
        ClearDropCue(); ClearGroupCue();
        DragPreviewLayer.Children.Clear(); _dragPreview = null;
        if (_sourceRow is not null)
        {
            if (_sourceOpacity == DependencyProperty.UnsetValue) _sourceRow.ClearValue(OpacityProperty);
            else _sourceRow.SetValue(OpacityProperty, _sourceOpacity);
        }
        _sourceRow = null; _sourceOpacity = DependencyProperty.UnsetValue;
    }
    private void OnSubtaskGripDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: TodoTaskViewModel task })
        {
            _draggedSubtask = task; _subtaskDragStart = e.GetPosition(this);
            var row = FindAncestor<Border>((DependencyObject)sender, element => element.Name == "SubtaskRow");
            _dragGrabOffset = row is null ? new Point() : e.GetPosition(row);
            e.Handled = true;
        }
    }
    private void OnSubtaskGripMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_draggedSubtask?.ParentId is not { } parentId || e.LeftButton != MouseButtonState.Pressed) return;
        var position = e.GetPosition(this);
        if (Math.Abs(position.X - _subtaskDragStart.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(position.Y - _subtaskDragStart.Y) < SystemParameters.MinimumVerticalDragDistance) return;
        e.Handled = true;
        try
        {
            BeginRowDrag(FindAncestor<Border>((DependencyObject)sender, row => row.Name == "SubtaskRow"), _draggedSubtask);
            System.Windows.DragDrop.DoDragDrop((DependencyObject)sender, new SubtaskDragItem(_draggedSubtask.Id, parentId), System.Windows.DragDropEffects.Move);
        }
        finally { EndRowDrag(); _draggedSubtask = null; }
    }
    private sealed record SubtaskDropTarget(Border Row, int Index, bool After);
    private SubtaskDropTarget? ResolveSubtaskDrop(Border surface, SubtaskDragItem source, Point pointer)
    {
        if (surface.DataContext is not TodoTaskViewModel parent || parent.Id != source.ParentId ||
            !parent.Subtasks.Any(child => child.Id == source.Id) || pointer.X < 0 || pointer.X > surface.ActualWidth || pointer.Y < 0 || pointer.Y > surface.ActualHeight) return null;
        var rows = SubtaskRows(surface).Where(row => row.IsVisible && ((TodoTaskViewModel)row.DataContext).Id != source.Id).ToArray();
        if (rows.Length == 0) return null;
        var row = rows.FirstOrDefault(item => pointer.Y < item.TranslatePoint(new Point(), surface).Y + item.ActualHeight / 2);
        var after = row is null; row ??= rows[^1];
        var destination = parent.Subtasks.IndexOf((TodoTaskViewModel)row.DataContext) + (after ? 1 : 0);
        if (parent.Subtasks.ToList().FindIndex(child => child.Id == source.Id) < destination) destination--;
        return new SubtaskDropTarget(row, destination, after);
    }
    private void OnSubtaskDragOver(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetData(typeof(TodoTaskViewModel)) is TodoTaskViewModel) return;
        e.Handled = true;
        AutoScroll(e); UpdateLayout();
        var target = sender is Border surface && e.Data.GetData(typeof(SubtaskDragItem)) is SubtaskDragItem source
            ? ResolveSubtaskDrop(surface, source, e.GetPosition(surface)) : null;
        ClearGroupCue();
        if (target is null) { ClearDropCue(); e.Effects = DragDropEffects.None; return; }
        ShowDropCue(target.Row, target.After); e.Effects = DragDropEffects.Move;
    }
    private void OnSubtaskDragLeave(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetData(typeof(SubtaskDragItem)) is not SubtaskDragItem || sender is not Border surface) return;
        e.Handled = true;
        var point = e.GetPosition(surface);
        if (point.X < 0 || point.Y < 0 || point.X > surface.ActualWidth || point.Y > surface.ActualHeight) ClearDropCue();
    }
    private void OnSubtaskDrop(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetData(typeof(TodoTaskViewModel)) is TodoTaskViewModel) return;
        e.Handled = true;
        var target = sender is Border surface && e.Data.GetData(typeof(SubtaskDragItem)) is SubtaskDragItem source
            ? ResolveSubtaskDrop(surface, source, e.GetPosition(surface)) : null;
        ClearDropCue();
        if (target is not null && e.Data.GetData(typeof(SubtaskDragItem)) is SubtaskDragItem item)
            ApplyAnimatedSubtaskMove(item.ParentId, () => ViewModel.MoveSubtask(item.Id, target.Index));
    }
    private static IEnumerable<Border> SubtaskRows(DependencyObject root) => DescendantsOf<Border>(root).Where(row => row.Name == "SubtaskRow");
    private void ApplyAnimatedSubtaskMove(Guid parentId, Action move)
    {
        var previous = SubtaskRows(this).Where(row => row.DataContext is TodoTaskViewModel task && task.ParentId == parentId)
            .ToDictionary(row => ((TodoTaskViewModel)row.DataContext).Id, row => row.TranslatePoint(new Point(), this).Y);
        move(); UpdateLayout();
        if (!AnimationSettings.AreAnimationsEnabled) return;
        foreach (var row in SubtaskRows(this).Where(row => row.DataContext is TodoTaskViewModel task && task.ParentId == parentId))
        {
            if (!previous.TryGetValue(((TodoTaskViewModel)row.DataContext).Id, out var oldY) || FindNamedDescendant<Grid>(row, "SubtaskRowContent") is not { } content) continue;
            var offset = oldY - row.TranslatePoint(new Point(), this).Y;
            if (Math.Abs(offset) < .5) continue;
            TaskDragVisualPolicy.EnsureMutableTranslateTransform(content).BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(offset, 0, TimeSpan.FromMilliseconds(180)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
        }
    }

    private void ClearGroupCue()
    {
        if (_groupDropTarget is not null) _groupDropTarget.Background = _groupOriginalBackground;
        _groupDropTarget = null;
    }

    private void ApplyAnimatedMove(Action move)
    {
        var previous = TaskRows(PendingItems).ToDictionary(
            row => ((TodoTaskViewModel)row.DataContext).Id,
            row => row.TranslatePoint(new System.Windows.Point(), PendingItems).Y);
        move();
        PendingItems.UpdateLayout();
        if (!AnimationSettings.AreAnimationsEnabled) return;
        foreach (var row in TaskRows(PendingItems))
        {
            var id = ((TodoTaskViewModel)row.DataContext).Id;
            if (!previous.TryGetValue(id, out var oldY)) continue;
            var offset = oldY - row.TranslatePoint(new System.Windows.Point(), PendingItems).Y;
            if (Math.Abs(offset) < 0.5 || FindNamedDescendant<Grid>(row, "TaskRowContent") is not { } content) continue;
            var transform = TaskDragVisualPolicy.EnsureMutableTranslateTransform(content);
            transform.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(offset, 0, TimeSpan.FromMilliseconds(180))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                });
        }
    }

    private static IEnumerable<Border> TaskRows(DependencyObject root)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is Border { Name: "TaskRowContainer" } row) yield return row;
            else foreach (var descendant in TaskRows(child)) yield return descendant;
        }
    }

    private void AutoScroll(System.Windows.DragEventArgs e)
    {
        if ((DateTime.UtcNow - _lastAutoScroll).TotalMilliseconds < 65) return;
        var point = e.GetPosition(TasksScrollViewer);
        var offset = point.Y < 30 ? -14 : point.Y > TasksScrollViewer.ActualHeight - 30 ? 14 : 0;
        if (offset == 0) return;
        _lastAutoScroll = DateTime.UtcNow;
        TasksScrollViewer.ScrollToVerticalOffset(TasksScrollViewer.VerticalOffset + offset);
    }

    private void ShowDragPreview(TodoTaskViewModel task)
    {
        var row = _sourceRow ?? TaskRows(PendingItems).Single(item => ((TodoTaskViewModel)item.DataContext).Id == task.Id);
        row.UpdateLayout();
        var dpi = VisualTreeHelper.GetDpi(row);
        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(row.ActualWidth * dpi.DpiScaleX),
            (int)Math.Ceiling(row.ActualHeight * dpi.DpiScaleY), 96 * dpi.DpiScaleX, 96 * dpi.DpiScaleY, PixelFormats.Pbgra32);
            var snapshot = new DrawingVisual();
            using (var drawing = snapshot.RenderOpen())
                drawing.DrawRectangle(new VisualBrush(row)
                {
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Viewbox = new Rect(0, 0, row.ActualWidth, row.ActualHeight),
                    Stretch = Stretch.Fill,
                }, null, new Rect(0, 0, row.ActualWidth, row.ActualHeight));
            bitmap.Render(snapshot);
        bitmap.Freeze();
        var colors = TodoThemePalette.Get(ViewModel.ColorTheme);
        var surface = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colors.Surface));
        var accent = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colors.Accent));
        var card = new Grid { Width = row.ActualWidth, Height = row.ActualHeight, IsHitTestVisible = false, Opacity = 0.96 };
        card.Children.Add(new Border { Background = surface, CornerRadius = new CornerRadius(10) });
        card.Children.Add(new System.Windows.Controls.Image { Source = bitmap, Width = row.ActualWidth, Height = row.ActualHeight });
        card.Children.Add(new Border { BorderBrush = accent, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(10) });
        System.Windows.Automation.AutomationProperties.SetName(card, task.Text);
        // An in-window overlay cannot steal OLE drag events like a floating HWND.
        _dragPreview = card;
        DragPreviewLayer.Children.Clear();
        DragPreviewLayer.Children.Add(card);
        UpdateDragPreviewPosition();
    }

    private void UpdateDragPreviewPosition()
    {
        if (_dragPreview is null) return;
        var cursor = System.Windows.Forms.Cursor.Position;
        var point = RootContainer.PointFromScreen(new System.Windows.Point(cursor.X, cursor.Y));
        Canvas.SetLeft(_dragPreview, point.X - _dragGrabOffset.X + 10);
        Canvas.SetTop(_dragPreview, point.Y - _dragGrabOffset.Y + 10);
    }

    private void OnDragGiveFeedback(object sender, System.Windows.GiveFeedbackEventArgs e)
    {
        UpdateDragPreviewPosition();
        e.UseDefaultCursors = true;
        e.Handled = true;
    }

    private static T? FindAncestor<T>(DependencyObject child, Func<T, bool> predicate) where T : DependencyObject
    {
        for (var current = VisualTreeHelper.GetParent(child); current is not null; current = VisualTreeHelper.GetParent(current))
            if (current is T match && predicate(match)) return match;
        return null;
    }

    private void OnPaletteClick(object sender, RoutedEventArgs e) => PalettePopup.IsOpen = true;

    private void OnThemeSelected(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string themeName } &&
            Enum.TryParse<TodoColorTheme>(themeName, ignoreCase: true, out var theme))
        {
            ViewModel.SelectThemeCommand.Execute(theme);
        }

        PalettePopup.IsOpen = false;
    }

    private void OnResizeDragDelta(object sender, DragDeltaEventArgs e)
    {
        Width = Math.Max(AppConstants.MinimumWindowWidth, Width + e.HorizontalChange);
        Height = Math.Max(AppConstants.MinimumWindowHeight, Height + e.VerticalChange);
    }

    private void OnResizeDragCompleted(object sender, DragCompletedEventArgs e) =>
        ViewModel.CommitBounds(Left, Top, ActualWidth, ActualHeight);

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (!AllowClose)
        {
            e.Cancel = true;
            ViewModel.HideCommand.Execute(null);
        }
    }

    private static bool IsInteractiveSource(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is ButtonBase or TextBox)
            {
                return true;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }

    private static T? FindDescendant<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match)
            {
                return match;
            }

            var descendant = FindDescendant<T>(child);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }

    private static T? FindNamedDescendant<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match && string.Equals(match.Name, name, StringComparison.Ordinal))
            {
                return match;
            }

            var descendant = FindNamedDescendant<T>(child, name);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }
}
