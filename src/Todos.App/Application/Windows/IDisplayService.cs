namespace Todos.Application.Windows;

public interface IDisplayService
{
    DisplayArea GetBestWorkingArea(double? x, double? y);
}
