namespace Todos.Presentation;

public static class AnimationSettings
{
    public static bool AreAnimationsEnabled => SystemParameters.ClientAreaAnimation;

    public static TimeSpan WindowEntranceDuration => TimeSpan.FromMilliseconds(180);
}
