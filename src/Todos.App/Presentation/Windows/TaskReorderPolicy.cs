namespace Todos.Presentation.Windows;

public static class TaskReorderPolicy
{
    public static int GetDestinationIndex(
        int sourceIndex,
        int targetIndex,
        bool insertAfterTarget,
        int itemCount)
    {
        if (itemCount <= 0)
        {
            return 0;
        }

        var insertionIndex = targetIndex + (insertAfterTarget ? 1 : 0);
        if (sourceIndex < insertionIndex)
        {
            insertionIndex--;
        }

        return Math.Clamp(insertionIndex, 0, itemCount - 1);
    }
}
