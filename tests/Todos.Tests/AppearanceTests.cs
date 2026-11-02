using Todos.Domain;
using Todos.Domain.Models;
using Todos.Infrastructure.Persistence;
using Todos.Presentation.ViewModels;

namespace Todos.Tests;

internal static class AppearanceTests
{
    [Test]
    public static async Task AppearanceSurvivesSnapshotAndRestartAndLegacyDefaultsStayMedium()
    {
        var state = new AppState();
        state.Settings.FontSizeStep = 3;
        state.Settings.LauncherSize = 2;
        state.Settings.SpacingSize = 0;
        var snapshot = AppStateSnapshot.Clone(state);
        state.Settings.FontSizeStep = 0;
        Assert.Equal(3, snapshot.Settings.FontSizeStep);
        using var directory = new PersistenceTests.TemporaryDirectory();
        var repository = new JsonAppStateRepository(new AppDataPaths(directory.Path));
        await repository.SaveAsync(snapshot);
        var loaded = (await repository.LoadAsync()).State.Settings;
        Assert.Equal(3, loaded.FontSizeStep);
        Assert.Equal(2, loaded.LauncherSize);
        Assert.Equal(0, loaded.SpacingSize);
        var defaults = AppStateNormalizer.Normalize(new AppState()).Settings;
        Assert.Equal(0, defaults.FontSizeStep);
        Assert.Equal(1, defaults.LauncherSize);
        Assert.Equal(1, defaults.SpacingSize);
    }

    [Test]
    public static void InvalidAppearanceIsNormalizedAndFontCommandsStopAtLimits()
    {
        var state = AppStateNormalizer.Normalize(new AppState
        { Settings = new AppSettings { FontSizeStep = 99, LauncherSize = -3, SpacingSize = 99 } });
        Assert.Equal(4, state.Settings.FontSizeStep);
        Assert.Equal(1, state.Settings.LauncherSize);
        Assert.Equal(1, state.Settings.SpacingSize);
        var changed = 0;
        var vm = new LauncherViewModel(false, () => { }, () => { }, () => { }, _ => { }, _ => { }, _ => true,
            () => Task.CompletedTask, appearanceSettings: state.Settings, appearanceChanged: () => changed++);
        Assert.False(vm.LargerFontCommand.CanExecute(null));
        for (var index = 0; index < 6; index++) vm.SmallerFontCommand.Execute(null);
        Assert.Equal("80 %", vm.FontSizeLabel);
        Assert.False(vm.SmallerFontCommand.CanExecute(null));
        vm.LauncherSmall = true;
        vm.SpacingLarge = true;
        Assert.Equal(0, state.Settings.LauncherSize);
        Assert.Equal(2, state.Settings.SpacingSize);
        Assert.Equal(8, changed);
    }
}
