using System.Threading;

namespace LionFire.Trading.Symbols;

/// <summary>
/// Repository interface for persisting symbol collection snapshots.
/// </summary>
public interface ISymbolCollectionRepository
{
    /// <summary>
    /// Saves a snapshot to storage.
    /// </summary>
    /// <param name="snapshot">The snapshot to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveAsync(SymbolCollectionSnapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a snapshot by ID.
    /// </summary>
    /// <param name="id">The snapshot ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The snapshot, or null if not found.</returns>
    Task<SymbolCollectionSnapshot?> LoadAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all available snapshots.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of snapshot metadata.</returns>
    Task<IReadOnlyList<SymbolCollectionSnapshot>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a snapshot by ID.
    /// </summary>
    /// <param name="id">The snapshot ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a snapshot with the given ID exists.
    /// </summary>
    /// <param name="id">The snapshot ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
}
