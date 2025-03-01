using LionFire.Execution;

namespace LionFire.Trading.Automation.Optimization;

public interface IOptimizationRepository
{
    Task<OptimizationRun> Load(OptimizationRunId id, ProgressListener? progressListener = null, bool refresh = false, CancellationToken cancellationToken = default);

    Task<OptimizationRunBacktests?> LoadBacktests(OptimizationRunId id, ProgressListener? progressListener = null, bool refresh = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<OptimizationRun>> GetRuns(string? botName = null);

    Task<OptimizationRunStats?> GetStats(OptimizationRunId id, ProgressListener? progressListener = null, bool refresh = false, CancellationToken cancellationToken = default);


    #region Notes

    Task<OptimizationRunNotes?> GetNotes(OptimizationRunId optimizationRun, ProgressListener? progressListener = null, bool refresh = false, CancellationToken cancellationToken = default);
    Task SetNotes(OptimizationRunId id, OptimizationRunNotes notes);

    #endregion

}
