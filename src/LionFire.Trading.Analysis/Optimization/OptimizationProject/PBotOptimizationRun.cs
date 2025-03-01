using DynamicData;
using ReactiveUI.SourceGenerators;

namespace LionFire.Trading;

public partial class PBotOptimizationRun : Buildable
{
    public Type? PBotType { get; set; }

    /// <summary>
    /// Do not consider backtests conducted before this date
    /// </summary>
    [Reactive]
    private DateTimeOffset? _backtestStartDate;

    /// <summary>
    /// Do not consider backtests conducted on or after this date
    /// </summary>
    [ReactiveUI.SourceGenerators.Reactive]
    private DateTimeOffset? _backtestEndExclusiveDate;

    /// <summary>
    /// Do not consider backtests for bots built before this date
    /// </summary>
    [ReactiveUI.SourceGenerators.Reactive]
    private DateTimeOffset? _botBuildStartDate;

    /// <summary>
    /// Do not consider backtests for bots built on or after this date
    /// </summary>
    [ReactiveUI.SourceGenerators.Reactive]
    private DateTimeOffset? _botBuildEndExclusiveDate;

    public SourceCache<BotAnalysisSnapshot, DateTimeOffset>? Snapshots { get; set; } // FUTURE - move to a cache object

}
