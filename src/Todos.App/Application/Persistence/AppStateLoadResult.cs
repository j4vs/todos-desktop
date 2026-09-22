using Todos.Domain.Models;

namespace Todos.Application.Persistence;

public sealed record AppStateLoadResult(
    AppState State,
    AppStateLoadSource Source,
    string? Warning = null);

public enum AppStateLoadSource
{
    Empty,
    Primary,
    Backup,
}
