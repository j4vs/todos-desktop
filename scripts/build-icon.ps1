param(
    [string] $SourcePath = (Join-Path $PSScriptRoot '..\src\Todos.App\Assets\icon.svg'),
    [string] $DestinationPath = (Join-Path $PSScriptRoot '..\src\Todos.App\Assets\icon.ico')
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationCore, WindowsBase

$source = [System.IO.Path]::GetFullPath($SourcePath)
$destination = [System.IO.Path]::GetFullPath($DestinationPath)
$sizes = @(16, 20, 24, 32, 40, 48, 64, 128, 256)

$isVector = [System.IO.Path]::GetExtension($source) -eq '.svg'
if ($isVector) {
    $svg = [xml](Get-Content -LiteralPath $source -Raw)
    $viewBox = @($svg.DocumentElement.GetAttribute('viewBox').Split(' ') | ForEach-Object { [double]::Parse($_, [cultureinfo]::InvariantCulture) })
    $brushConverter = [System.Windows.Media.BrushConverter]::new()
}
else {
    $bitmap = [System.Windows.Media.Imaging.BitmapImage]::new()
    $bitmap.BeginInit()
    $bitmap.CacheOption = [System.Windows.Media.Imaging.BitmapCacheOption]::OnLoad
    $bitmap.UriSource = [Uri]::new($source)
    $bitmap.EndInit()
    $bitmap.Freeze()
}

function Render-Icon([int]$size) {
    $visual = [System.Windows.Media.DrawingVisual]::new()
    $context = $visual.RenderOpen()
    if ($isVector) {
        $context.PushTransform([System.Windows.Media.ScaleTransform]::new($size / $viewBox[2], $size / $viewBox[3]))
        foreach ($element in $svg.DocumentElement.ChildNodes) {
            if ($element -isnot [System.Xml.XmlElement]) { continue }
            $fill = $brushConverter.ConvertFromString($element.GetAttribute('fill'))
            switch ($element.LocalName) {
                'circle' {
                    $stroke = $brushConverter.ConvertFromString($element.GetAttribute('stroke'))
                    $pen = [System.Windows.Media.Pen]::new($stroke, [double]::Parse($element.GetAttribute('stroke-width'), [cultureinfo]::InvariantCulture))
                    $center = [System.Windows.Point]::new([double]$element.GetAttribute('cx'), [double]$element.GetAttribute('cy'))
                    $radius = [double]$element.GetAttribute('r')
                    $context.DrawEllipse($fill, $pen, $center, $radius, $radius)
                }
                'path' {
                    $geometry = [System.Windows.Media.Geometry]::Parse($element.GetAttribute('d')).Clone()
                    if ($element.HasAttribute('transform')) {
                        $matrixText = $element.GetAttribute('transform').Replace('matrix(', '').Replace(')', '')
                        $geometry.Transform = [System.Windows.Media.MatrixTransform]::new([System.Windows.Media.Matrix]::Parse($matrixText))
                    }
                    $context.DrawGeometry($fill, $null, $geometry)
                }
                default { throw "Elemento SVG no soportado por el icono: $($element.LocalName)" }
            }
        }
        $context.Pop()
    }
    else {
        [System.Windows.Media.RenderOptions]::SetBitmapScalingMode($visual, [System.Windows.Media.BitmapScalingMode]::HighQuality)
        $context.DrawImage($bitmap, [System.Windows.Rect]::new(0, 0, $size, $size))
    }
    $context.Close()
    $render = [System.Windows.Media.Imaging.RenderTargetBitmap]::new(
        $size,
        $size,
        96,
        96,
        [System.Windows.Media.PixelFormats]::Pbgra32)
    $render.Render($visual)
    $encoder = [System.Windows.Media.Imaging.PngBitmapEncoder]::new()
    $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($render))
    $stream = [System.IO.MemoryStream]::new()
    $encoder.Save($stream)
    $bytes = $stream.ToArray()
    $stream.Dispose()
    return ,$bytes
}

# Render every ICO size directly from the vector, including small tray sizes.
$images = foreach ($size in $sizes) {
    [pscustomobject]@{ Size = $size; Bytes = (Render-Icon $size) }
}
if ($isVector) {
    $pngPath = [System.IO.Path]::ChangeExtension($source, '.png')
    [System.IO.File]::WriteAllBytes($pngPath, (Render-Icon 512))
}

$outputDirectory = [System.IO.Path]::GetDirectoryName($destination)
[System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
$file = [System.IO.File]::Open($destination, [System.IO.FileMode]::Create)
$writer = [System.IO.BinaryWriter]::new($file)
try {
    $writer.Write([UInt16]0)
    $writer.Write([UInt16]1)
    $writer.Write([UInt16]$images.Count)

    $offset = 6 + (16 * $images.Count)
    foreach ($image in $images) {
        $dimension = if ($image.Size -eq 256) { 0 } else { $image.Size }
        $writer.Write([Byte]$dimension)
        $writer.Write([Byte]$dimension)
        $writer.Write([Byte]0)
        $writer.Write([Byte]0)
        $writer.Write([UInt16]1)
        $writer.Write([UInt16]32)
        $writer.Write([UInt32]$image.Bytes.Length)
        $writer.Write([UInt32]$offset)
        $offset += $image.Bytes.Length
    }

    foreach ($image in $images) {
        $writer.Write($image.Bytes)
    }
}
finally {
    $writer.Dispose()
    $file.Dispose()
}

Write-Host "Icono generado con $($images.Count) tamaños: $destination"
