using Todos.Application.Persistence;
using Todos.Domain;
using Todos.Domain.Models;
using Todos.Infrastructure.Persistence;
using Todos.Presentation.Controls;
using Todos.Presentation.ViewModels;

namespace Todos.Tests;

internal static class NoteTests
{
    internal const string Markdown = "# Módulo API\n\n**Importante** y *detalle*.\n\n- [x] Crear endpoints\n- [ ] Probar captura\n\n> Revisar con el equipo\n\n| Campo | Valor |\n| --- | --- |\n| Estado | Pendiente |\n\n```php\n  $id = 1;\n  echo $id;\n```\n\n[Documentación](https://example.org)\n\n---\n\n![Referencia](https://example.org/no-descargar.png)\n";

    [Test]
    public static void PreviousSchemasAndNullNotesMigrateWithoutChangingMarkdown()
    {
        foreach (var schema in Enumerable.Range(1, 4))
        {
            var root = new TodoTaskItem { Text = "Tarea", NotesMarkdown = null!, Subtasks = [new TodoTaskItem { Text = "Paso", NotesMarkdown = Markdown }] };
            var state = AppStateNormalizer.Normalize(new AppState { SchemaVersion = schema, TodoWindows = [new TodoWindowState { Tasks = [root] }] });
            Assert.Equal(5, state.SchemaVersion); Assert.Equal(string.Empty, root.NotesMarkdown);
            Assert.Equal(Markdown, root.Subtasks[0].NotesMarkdown);
        }
        var damaged = new TodoTaskItem { Text = "", NotesMarkdown = Markdown, Subtasks = [new TodoTaskItem { Text = " ", NotesMarkdown = "paso conservado" }] };
        var repaired = AppStateNormalizer.Normalize(new AppState { TodoWindows = [new TodoWindowState { Tasks = [damaged] }] });
        Assert.Equal(Markdown, repaired.TodoWindows[0].Tasks.Single().NotesMarkdown);
        Assert.Equal("paso conservado", repaired.TodoWindows[0].Tasks.Single().Subtasks.Single().NotesMarkdown);
    }

    [Test]
    public static async Task NotesSurviveEditingCompletionUndoMoveTrashAndRestart()
    {
        var state = new AppState(); var list = new TodoWindowState(); state.TodoWindows.Add(list);
        var root = TodoListOperations.AddTask(list, "Entrega", DateTimeOffset.UtcNow);
        var child = SubtaskOperations.Add(list, root.Id, "Paso", DateTimeOffset.UtcNow);
        var saveCalls = 0;
        using var vm = new TodoWindowViewModel(list, () => saveCalls++, () => { }, () => { });
        var rootVm = vm.PendingTasks.Single(); var childVm = rootVm.Subtasks.Single();
        Assert.False(rootVm.HasNotes); Assert.Equal("Agregar nota", rootVm.NotesMenuLabel);
        rootVm.NotesMarkdown = Markdown; childVm.NotesMarkdown = "\n    nota de paso\n";
        Assert.Equal(2, saveCalls); Assert.True(rootVm.HasNotes); Assert.Equal("Ver nota", rootVm.NotesMenuLabel);
        Assert.Equal("Guardando…", rootVm.NotesSaveStatus);
        var snapshot = AppStateSnapshot.Clone(state); rootVm.NotesMarkdown = "cambio";
        Assert.Equal(Markdown, snapshot.TodoWindows[0].Tasks[0].NotesMarkdown); rootVm.NotesMarkdown = Markdown;
        rootVm.IsCompleted = true; vm.UndoDeleteCommand.Execute(null);
        Assert.Equal(Markdown, root.NotesMarkdown);
        childVm.DeleteCommand.Execute(null); vm.UndoDeleteCommand.Execute(null);
        Assert.Equal("\n    nota de paso\n", root.Subtasks.Single().NotesMarkdown);
        var group = TodoListOperations.AddGroup(list, "App", DateTimeOffset.UtcNow);
        vm.MoveTaskToGroup(root.Id, group.Id);
        rootVm.DeleteCommand.Execute(null); vm.UndoDeleteCommand.Execute(null);
        Assert.Equal(Markdown, list.Tasks.Single().NotesMarkdown);
        RecycleBinOperations.MoveToTrash(state, list.Id, DateTimeOffset.UtcNow);
        using var directory = new PersistenceTests.TemporaryDirectory();
        var repository = new JsonAppStateRepository(new AppDataPaths(directory.Path));
        await repository.SaveAsync(state);
        var restored = (await repository.LoadAsync()).State;
        RecycleBinOperations.Restore(restored, list.Id);
        var savedRoot = restored.TodoWindows.Single().Tasks.Single();
        Assert.Equal(root.Id, savedRoot.Id); Assert.Equal(child.Id, savedRoot.Subtasks.Single().Id);
        Assert.Equal(Markdown, savedRoot.NotesMarkdown); Assert.Equal("\n    nota de paso\n", savedRoot.Subtasks.Single().NotesMarkdown);
        Assert.Equal(group.Id, savedRoot.GroupId);
    }

    [Test]
    public static void MarkdownFilesPreserveUtf8WhitespaceAndRejectInvalidInput()
    {
        using var directory = new PersistenceTests.TemporaryDirectory();
        var path = System.IO.Path.Combine(directory.Path, "nota.md");
        MarkdownNoteFiles.Write(path, Markdown); Assert.Equal(Markdown, MarkdownNoteFiles.Read(path));
        MarkdownNoteFiles.Write(path, "reemplazo\r\n\táé🙂\r\n");
        Assert.Equal("reemplazo\r\n\táé🙂\r\n", MarkdownNoteFiles.Read(path));
        File.WriteAllBytes(path, [0xC3, 0x28]);
        Assert.Throws<System.Text.DecoderFallbackException>(() => MarkdownNoteFiles.Read(path));
        Assert.False(Directory.GetFiles(directory.Path).Any(file => file.EndsWith(".tmp")));
    }

    [Test]
    public static async Task SaveNotificationReflectsTheActualSnapshotAndDiskFailure()
    {
        using var directory = new PersistenceTests.TemporaryDirectory();
        var paths = new AppDataPaths(directory.Path); var repository = new JsonAppStateRepository(paths);
        var state = new AppState { TodoWindows = [new TodoWindowState { Tasks = [new TodoTaskItem { Text = "Tarea", NotesMarkdown = "anterior" }] }] };
        await repository.SaveAsync(state);
        await using var saver = new DebouncedAppStateSaver(repository);
        AppState? saved = null; Exception? error = null;
        saver.SaveCompleted += (snapshot, failure) => { saved = snapshot; error = failure; };
        state.TodoWindows[0].Tasks[0].NotesMarkdown = Markdown;
        saver.Schedule(state, TimeSpan.FromSeconds(30)); state.TodoWindows[0].Tasks[0].NotesMarkdown = "cambio posterior";
        await saver.FlushAsync();
        Assert.Equal(Markdown, saved!.TodoWindows[0].Tasks[0].NotesMarkdown); Assert.Equal<Exception?>(null, error);
        await using (var locked = new FileStream(paths.TemporaryFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
        {
            saver.Schedule(state, TimeSpan.FromSeconds(30));
            await Assert.ThrowsAsync<IOException>(() => saver.FlushAsync()); Assert.NotNull(error);
        }
        Assert.Equal(Markdown, (await repository.LoadAsync()).State.TodoWindows[0].Tasks[0].NotesMarkdown);
    }

    [Test]
    public static void ViewerOnlyAllowsExplicitWebAndMailLinks()
    {
        Assert.True(MarkdownNoteRenderer.IsExternalLink("https://example.org"));
        Assert.True(MarkdownNoteRenderer.IsExternalLink("mailto:usuario@example.org"));
        foreach (var url in new[] { "javascript:alert(1)", "file:///C:/Windows/notepad.exe", "cmd:run", "../archivo.md", "" })
            Assert.False(MarkdownNoteRenderer.IsExternalLink(url));
    }
}
