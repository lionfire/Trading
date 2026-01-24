using System.Threading;

namespace LionFire.Trading.Optimization.Execution;

/// <summary>
/// Repository for persisting plan execution state for pause/resume capability.
/// </summary>
public interface IPlanExecutionStateRepository
{
    /// <summary>
    /// Save the current execution state.
    /// </summary>
    /// <param name="state">State to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveAsync(PlanExecutionState state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Load the execution state for a plan, if it exists.
    /// </summary>
    /// <param name="planId">ID of the plan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Persisted state, or null if no state exists.</returns>
    Task<PlanExecutionState?> LoadAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete the persisted state for a plan.
    /// </summary>
    /// <param name="planId">ID of the plan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if persisted state exists for a plan.
    /// </summary>
    /// <param name="planId">ID of the plan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<bool> ExistsAsync(string planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// List all persisted execution states.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All persisted states.</returns>
    Task<IReadOnlyList<PlanExecutionState>> ListAsync(CancellationToken cancellationToken = default);
}
