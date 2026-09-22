namespace Todos.Application.Windows;

public interface IStartupRegistrationService
{
    StartupRegistrationStatus GetStatus();

    void Enable();

    void Disable();
}

public sealed record StartupRegistrationStatus(bool IsRegistered, bool MatchesCurrentExecutable, string? RegisteredCommand);
