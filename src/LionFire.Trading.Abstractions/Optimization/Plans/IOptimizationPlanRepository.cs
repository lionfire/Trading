using System.Threading;

namespace LionFire.Trading.Optimization.Plans;

/// <summary>
/// Repository for storing and retrieving optimization plans.
/// </summary>
public interface IOptimizationPlanRepository
{
    /// <summary>
    /// Gets a plan by ID.
    /// </summary>
    Task<OptimizationPlan?> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all plans.
    /// </summary>
    Task<IReadOnlyList<OptimizationPlan>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a plan (creates or updates). Auto-increments version on update.
    /// </summary>
    Task<OptimizationPlan> SaveAsync(OptimizationPlan plan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a plan by ID.
    /// </summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a plan exists.
    /// </summary>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
}
