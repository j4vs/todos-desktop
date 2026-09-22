using Todos.Domain.Models;

namespace Todos.Application.Persistence;

public interface IAppStateRepository
{
    Task<AppStateLoadResult> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(AppState state, CancellationToken cancellationToken = default);
}
