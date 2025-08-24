using Orleans;

namespace LionFire.Trading.Optimization;

[GenerateSerializer]
public class OptimizationProgress
{
    [Id(0)] public long PlannedScanTotal { get; set; }
    [Id(1)] public long ComprehensiveScanTotal { get; set; }
    public double ComprehensiveScanPerUn => ComprehensiveScanTotal == 0 ? 0 : (PlannedScanTotal / ComprehensiveScanTotal);

    [Id(2)] public long Skipped { get; set; }
    [Id(3)] public long Queued { get; set; }
    [Id(4)] public long FractionallyCompleted { get; set; }
    [Id(5)] public long Completed { get; set; }
    public double FractionalPercent => FractionalPerUn * 100.0;
    public double Percent => PerUn * 100.0;
    public double PerUn => Queued == 0 ? 0 : (double)Completed / Queued;
    public double FractionalPerUn => Queued == 0 ? 0 : (double)FractionallyCompleted / Queued;
    public long Remaining => Queued - Completed;
    [Id(6)] public DateTimeOffset? Start { get; set; }
    [Id(7)] public DateTimeOffset? EstimatedEnd { get; set; }
    public TimeSpan? EstimatedDuration => EstimatedEnd - Start;

    [Id(8)] public TimeSpan PauseElapsed { get; set; }
    [Id(9)] public bool IsPaused { get; set; }

    public static readonly OptimizationProgress NoProgress = new();
}