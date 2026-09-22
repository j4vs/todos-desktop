using System.Collections.Concurrent;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Linq;
using MediaBrush = System.Windows.Media.Brush;

namespace Todos.Presentation.Controls;

public sealed class SvgIcon : FrameworkElement
{
    private static readonly ConcurrentDictionary<string, SvgDrawingData> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        nameof(Source),
        typeof(string),
        typeof(SvgIcon),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(
        typeof(SvgIcon),
        new FrameworkPropertyMetadata(System.Windows.Media.Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
        nameof(StrokeThickness),
        typeof(double),
        typeof(SvgIcon),
        new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    public string Source
    {
        get => (string)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public MediaBrush Foreground
    {
        get => (MediaBrush)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        if (string.IsNullOrWhiteSpace(Source) || ActualWidth <= 0 || ActualHeight <= 0)
        {
            return;
        }

        var data = Cache.GetOrAdd(Source, Load);
        var scale = Math.Min(ActualWidth / data.Width, ActualHeight / data.Height);
        var offsetX = (ActualWidth - (data.Width * scale)) / 2;
        var offsetY = (ActualHeight - (data.Height * scale)) / 2;

        drawingContext.PushTransform(new TranslateTransform(offsetX, offsetY));
        drawingContext.PushTransform(new ScaleTransform(scale, scale));
        var outline = StrokeThickness > 0
            ? new System.Windows.Media.Pen(Foreground, StrokeThickness)
            {
                LineJoin = PenLineJoin.Round,
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
            }
            : null;
        foreach (var geometry in data.Geometries)
        {
            drawingContext.DrawGeometry(Foreground, outline, geometry);
        }

        drawingContext.Pop();
        drawingContext.Pop();
    }

    private static SvgDrawingData Load(string source)
    {
        var normalizedSource = source.TrimStart('/').Replace('\\', '/');
        var uri = new Uri($"pack://application:,,,/Todos;component/{normalizedSource}", UriKind.Absolute);
        var resource = System.Windows.Application.GetResourceStream(uri)
            ?? throw new InvalidOperationException($"No se encontró el icono '{source}'.");

        using var stream = resource.Stream;
        var document = XDocument.Load(stream);
        var root = document.Root ?? throw new FormatException($"El icono '{source}' está vacío.");
        var viewBox = (root.Attribute("viewBox")?.Value ?? "0 0 24 24")
            .Split([' ', ','], StringSplitOptions.RemoveEmptyEntries)
            .Select(value => double.Parse(value, CultureInfo.InvariantCulture))
            .ToArray();
        if (viewBox.Length != 4 || viewBox[2] <= 0 || viewBox[3] <= 0)
        {
            throw new FormatException($"El icono '{source}' tiene un viewBox inválido.");
        }

        var geometries = root.Descendants()
            .Where(element => element.Name.LocalName == "path")
            .Select(element => element.Attribute("d")?.Value)
            .Where(data => !string.IsNullOrWhiteSpace(data))
            .Select(data => Geometry.Parse(data!))
            .ToArray();
        if (geometries.Length == 0)
        {
            throw new FormatException($"El icono '{source}' no contiene trazos.");
        }

        foreach (var geometry in geometries)
        {
            geometry.Freeze();
        }

        return new SvgDrawingData(viewBox[2], viewBox[3], geometries);
    }

    private sealed record SvgDrawingData(double Width, double Height, Geometry[] Geometries);
}
