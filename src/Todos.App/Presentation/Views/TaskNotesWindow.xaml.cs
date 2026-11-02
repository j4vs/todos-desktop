using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Todos.Domain.Models;
using Todos.Infrastructure.Persistence;
using Todos.Presentation.Controls;
using Todos.Presentation.Themes;
using FontFamily = System.Windows.Media.FontFamily;
using Brush = System.Windows.Media.Brush;

namespace Todos.Presentation.Views;

public partial class TaskNotesWindow : System.Windows.Window
{
    private readonly ViewModels.TodoTaskViewModel _task;
    private string? _pendingImport;
    private bool _closed;
    public bool IsFileDialogOpen { get; private set; }

    public TaskNotesWindow(ViewModels.TodoTaskViewModel task, TodoColorTheme theme)
    {
        _task = task;
        InitializeComponent(); DataContext = task; ApplyTheme(theme);
        NotesTabs.SelectedIndex = task.HasNotes ? 0 : 1;
        task.PropertyChanged += OnTaskChanged;
        AppearanceResources.Changed += OnAppearanceChanged;
        Closed += (_, _) => { _closed = true; task.PropertyChanged -= OnTaskChanged; AppearanceResources.Changed -= OnAppearanceChanged; };
        Loaded += (_, _) => { RefreshPreview(); if (NotesTabs.SelectedIndex == 1) NoteEditor.Focus(); };
    }
    public void ApplyTheme(TodoColorTheme theme) { WindowThemeResources.Apply(Resources, theme); if (IsLoaded) RefreshPreview(); }
    private void OnAppearanceChanged() { if (IsLoaded) RefreshPreview(); }
    private void OnTaskChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_task.NotesMarkdown) && NotesTabs.SelectedIndex == 0) RefreshPreview();
    }
    private void RefreshPreview()
    {
        NotePreview.Document = MarkdownNoteRenderer.Render(_task.NotesMarkdown,
            (FontFamily)FindResource("TodosFontFamily"), (double)FindResource("Font14"),
            (Brush)FindResource("TodosTextBrush"), (Brush)FindResource("TodosBrandBrush"), (Brush)FindResource("TodosHoverSurfaceBrush"));
    }
    private void OnTabChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (!IsLoaded || e.Source != NotesTabs) return;
        if (NotesTabs.SelectedIndex == 0) RefreshPreview(); else NoteEditor.Focus();
    }
    private void OnClose(object sender, RoutedEventArgs e) => Close();
    private void OnHeaderMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is System.Windows.Controls.TextBlock && e.LeftButton == MouseButtonState.Pressed) DragMove();
    }
    private void OnResize(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
    { Width = Math.Max(MinWidth, Width + e.HorizontalChange); Height = Math.Max(MinHeight, Height + e.VerticalChange); }
    private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Escape) return;
        if (FileMessagePanel.IsVisible) OnCancelImport(sender, e); else Close();
        e.Handled = true;
    }
    private async void OnImport(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Markdown (*.md;*.markdown)|*.md;*.markdown", CheckFileExists = true };
        bool? selected;
        IsFileDialogOpen = true;
        try { selected = dialog.ShowDialog(this); } finally { IsFileDialogOpen = false; }
        if (selected != true) return;
        SetFileBusy(true);
        try
        {
            var content = await Task.Run(() => MarkdownNoteFiles.Read(dialog.FileName));
            if (_closed) return;
            StageImport(content);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Text.DecoderFallbackException)
        { ShowFileError("No se pudo leer el archivo Markdown. La nota actual se conserva."); }
        finally { SetFileBusy(false); }
    }
    internal void StageImport(string content)
    {
        if (!_task.HasNotes) { ApplyImport(content); return; }
        _pendingImport = content;
        FileMessage.Text = "Importar reemplazará el contenido de esta nota. ¿Quieres continuar?";
        ConfirmImportButton.Visibility = Visibility.Visible; CancelImportButton.Content = "Cancelar";
        FileMessagePanel.Visibility = Visibility.Visible; CancelImportButton.Focus();
    }
    private void ApplyImport(string content) { _task.NotesMarkdown = content; NotesTabs.SelectedIndex = 1; NoteEditor.Focus(); }
    private void OnConfirmImport(object sender, RoutedEventArgs e)
    {
        var content = _pendingImport; _pendingImport = null; FileMessagePanel.Visibility = Visibility.Collapsed;
        if (content is not null) ApplyImport(content);
    }
    private void OnCancelImport(object sender, RoutedEventArgs e) { _pendingImport = null; FileMessagePanel.Visibility = Visibility.Collapsed; }
    private async void OnExport(object sender, RoutedEventArgs e)
    {
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        var name = new string(_task.Text.Select(character => invalid.Contains(character) ? '_' : character).ToArray());
        var dialog = new Microsoft.Win32.SaveFileDialog { Filter = "Markdown (*.md)|*.md", DefaultExt = ".md", AddExtension = true,
            FileName = name.Length > 70 ? name[..70] : name, OverwritePrompt = true };
        bool? selected;
        IsFileDialogOpen = true;
        try { selected = dialog.ShowDialog(this); } finally { IsFileDialogOpen = false; }
        if (selected != true) return;
        var content = _task.NotesMarkdown;
        SetFileBusy(true);
        try { await Task.Run(() => MarkdownNoteFiles.Write(dialog.FileName, content)); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        { ShowFileError("No se pudo exportar la nota. El contenido sigue en la aplicación."); }
        finally { SetFileBusy(false); }
    }
    private void SetFileBusy(bool busy) { ImportButton.IsEnabled = ExportButton.IsEnabled = !busy; }
    private void ShowFileError(string message)
    {
        if (_closed) return;
        FileMessage.Text = message; ConfirmImportButton.Visibility = Visibility.Collapsed;
        CancelImportButton.Content = "Cerrar"; FileMessagePanel.Visibility = Visibility.Visible; CancelImportButton.Focus();
    }
}
