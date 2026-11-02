using System.Text;

namespace Todos.Infrastructure.Persistence;

public static class MarkdownNoteFiles
{
    public static string Read(string path) => File.ReadAllText(path, new UTF8Encoding(false, true));

    public static void Write(string path, string markdown)
    {
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, markdown, new UTF8Encoding(false));
            File.Move(temporary, path, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}
