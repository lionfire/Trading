namespace LionFire.Trading.Optimization.Matrix;

/// <summary>
/// Tracks Level of Detail (LoD) progression state for a single matrix cell.
/// </summary>
public record CellLodState
{
    /// <summary>
    /// Best (highest, closest to 0) LoD level completed for this cell.
    /// 0 = comprehensive, negative = coarser. Null if never run.
    /// </summary>
    public int? AchievedLevel { get; init; }

    /// <summary>
    /// Total backtests completed at the achieved level.
    /// </summary>
    public long BacktestsCompleted { get; init; }

    /// <summary>
    /// LoD level currently queued for execution. Null if nothing queued.
    /// </summary>
    public int? QueuedLevel { get; init; }
}
