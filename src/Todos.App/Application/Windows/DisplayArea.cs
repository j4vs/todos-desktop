namespace Todos.Application.Windows;

public readonly record struct DisplayArea(double Left, double Top, double Width, double Height)
{
    public double Right => Left + Width;

    public double Bottom => Top + Height;
}
