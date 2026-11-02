using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Markdig.Extensions.TaskLists;
using MdTable = Markdig.Extensions.Tables.Table;
using MdRow = Markdig.Extensions.Tables.TableRow;
using MdCell = Markdig.Extensions.Tables.TableCell;
using WpfBlock = System.Windows.Documents.Block;
using WpfInline = System.Windows.Documents.Inline;
using FontFamily = System.Windows.Media.FontFamily;
using Brush = System.Windows.Media.Brush;

namespace Todos.Presentation.Controls;

public static class MarkdownNoteRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UsePipeTables().UseTaskLists().UseEmphasisExtras().UseAutoLinks().DisableHtml().Build();

    public static FlowDocument Render(string markdown, FontFamily font, double fontSize, Brush foreground, Brush accent, Brush surface)
    {
        var document = new FlowDocument { FontFamily = font, FontSize = fontSize, Foreground = foreground,
            PagePadding = new Thickness(12), ColumnWidth = double.PositiveInfinity };
        foreach (var block in Markdown.Parse(markdown, Pipeline)) document.Blocks.Add(ConvertBlock(block));
        if (document.Blocks.Count == 0) document.Blocks.Add(new Paragraph(new Run("Esta nota está vacía. Abre Editar para escribir.")));
        return document;

        WpfBlock ConvertBlock(Markdig.Syntax.Block block)
        {
            switch (block)
            {
                case HeadingBlock heading:
                    var title = Paragraph(heading);
                    title.FontSize = fontSize * (heading.Level <= 2 ? 1.55 : 1.2);
                    title.FontWeight = FontWeights.SemiBold;
                    title.Margin = new Thickness(0, 10, 0, 8);
                    return title;
                case CodeBlock code:
                    return new Paragraph(new Run(code.Lines.ToString())) { FontFamily = new FontFamily("Consolas"),
                        Background = surface, Padding = new Thickness(10), Margin = new Thickness(0, 6, 0, 10) };
                case QuoteBlock quote:
                    var citation = Container(quote);
                    citation.BorderBrush = accent; citation.BorderThickness = new Thickness(3, 0, 0, 0);
                    citation.Padding = new Thickness(12, 4, 4, 4); citation.Margin = new Thickness(0, 6, 0, 10);
                    return citation;
                case ListBlock list:
                    var result = new System.Windows.Documents.List { MarkerStyle = list.IsOrdered ? TextMarkerStyle.Decimal : TextMarkerStyle.Disc,
                        Margin = new Thickness(0, 2, 0, 8), Padding = new Thickness(24, 0, 0, 0) };
                    if (list.OfType<ListItemBlock>().All(item => item.FirstOrDefault() is ParagraphBlock paragraph && paragraph.Inline?.Any(inline => inline is TaskList) == true))
                        result.MarkerStyle = TextMarkerStyle.None;
                    if (list.IsOrdered && int.TryParse(list.OrderedStart, out var start) && start > 0) result.StartIndex = start;
                    foreach (var item in list.OfType<ListItemBlock>())
                    {
                        var renderedItem = new ListItem(); renderedItem.Blocks.Add(Container(item)); result.ListItems.Add(renderedItem);
                    }
                    return result;
                case ThematicBreakBlock:
                    return new Paragraph { BorderBrush = accent, BorderThickness = new Thickness(0, 1, 0, 0), Margin = new Thickness(0, 8, 0, 8), FontSize = 1 };
                case MdTable table:
                    var rendered = new System.Windows.Documents.Table { CellSpacing = 0, Margin = new Thickness(0, 8, 0, 10) };
                    for (var index = 0; index < table.ColumnDefinitions.Count; index++) rendered.Columns.Add(new TableColumn());
                    var rows = new TableRowGroup(); rendered.RowGroups.Add(rows);
                    foreach (var row in table.OfType<MdRow>())
                    {
                        var renderedRow = new TableRow(); rows.Rows.Add(renderedRow);
                        foreach (var cell in row.OfType<MdCell>())
                        {
                            var renderedCell = new TableCell(Container(cell)) { Padding = new Thickness(7), BorderBrush = accent,
                                BorderThickness = new Thickness(.5), ColumnSpan = Math.Max(1, cell.ColumnSpan) };
                            if (row.IsHeader) { renderedCell.FontWeight = FontWeights.SemiBold; renderedCell.Background = surface; }
                            renderedRow.Cells.Add(renderedCell);
                        }
                    }
                    return rendered;
                case LeafBlock leaf: return Paragraph(leaf);
                case ContainerBlock container: return Container(container);
                default: return new Paragraph();
            }
        }
        Section Container(ContainerBlock block)
        {
            var section = new Section();
            foreach (var child in block) section.Blocks.Add(ConvertBlock(child));
            return section;
        }
        Paragraph Paragraph(LeafBlock block)
        {
            var paragraph = new Paragraph { Margin = new Thickness(0, 2, 0, 8) };
            if (block.Inline is { } inline) Append(paragraph.Inlines, inline);
            else paragraph.Inlines.Add(new Run(block.Lines.ToString()));
            return paragraph;
        }
        void Append(InlineCollection target, ContainerInline container)
        {
            foreach (var inline in container)
            {
                WpfInline? converted = inline switch
                {
                    LiteralInline literal => new Run(literal.Content.ToString()),
                    CodeInline code => new Run(code.Content) { FontFamily = new FontFamily("Consolas"), Background = surface },
                    LineBreakInline line => line.IsHard ? new LineBreak() : new Run(" "),
                    TaskList task => new Run(task.Checked ? "☑ " : "☐ ") { Foreground = accent, FontFamily = new FontFamily("Segoe UI Symbol") },
                    AutolinkInline link => MakeLink(link.Url, link.IsEmail ? "mailto:" + link.Url : link.Url),
                    _ => null,
                };
                if (converted is not null) { target.Add(converted); continue; }
                if (inline is LinkInline linkInline)
                {
                    // Images remain text; the viewer never loads local or remote assets.
                    if (linkInline.IsImage) { target.Add(new Run("[Imagen: ")); Append(target, linkInline); target.Add(new Run("]")); }
                    else
                    {
                        var link = MakeLink(null, linkInline.Url);
                        Append(link.Inlines, linkInline); target.Add(link);
                    }
                }
                else if (inline is EmphasisInline emphasis)
                {
                    Span span = emphasis.DelimiterChar == '~' ? new Span { TextDecorations = TextDecorations.Strikethrough }
                        : emphasis.DelimiterCount >= 2 ? new Bold() : new Italic();
                    Append(span.Inlines, emphasis); target.Add(span);
                }
                else if (inline is ContainerInline nested) Append(target, nested);
            }
        }
        Hyperlink MakeLink(string? text, string? url)
        {
            var link = new Hyperlink { Foreground = accent };
            if (text is not null) link.Inlines.Add(new Run(text));
            if (IsExternalLink(url))
            {
                link.NavigateUri = new Uri(url!); link.ToolTip = url;
                link.RequestNavigate += (_, e) =>
                {
                    try { Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true }); }
                    catch (System.ComponentModel.Win32Exception) { link.ToolTip = "No se pudo abrir el enlace."; }
                    e.Handled = true;
                };
            }
            return link;
        }
    }

    public static bool IsExternalLink(string? url) => Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && uri.Scheme is "http" or "https" or "mailto";
}
