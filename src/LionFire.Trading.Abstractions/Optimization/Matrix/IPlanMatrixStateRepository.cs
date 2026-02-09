namespace LionFire.Trading.Optimization.Matrix;

/// <summary>
/// Repository for persisting plan matrix state.
/// </summary>
public interface IPlanMatrixStateRepository
{
    /// <summary>
    /// Load the matrix state for a plan, if it exists.
    /// </summary>
    Task<PlanMatrixState?> LoadAsync(string planId);

    /// <summary>
    /// Save the matrix state.
    /// </summary>
    Task SaveAsync(PlanMatrixState state);

    /// <summary>
    /// Delete the persisted matrix state for a plan.
    /// </summary>
    Task DeleteAsync(string planId);
}
