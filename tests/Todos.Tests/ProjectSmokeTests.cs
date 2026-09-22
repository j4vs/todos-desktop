namespace Todos.Tests;

internal static class ProjectSmokeTests
{
    [Test]
    public static void ApplicationAssemblyUsesStableTechnicalName()
    {
        Assert.Equal("Todos", typeof(Todos.App.App).Assembly.GetName().Name);
    }
}
