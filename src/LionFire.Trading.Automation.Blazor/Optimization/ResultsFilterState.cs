using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.ComponentModel;

namespace LionFire.Trading.Automation.Blazor.Optimization;

/// <summary>
/// Reactive filter state for optimization results.
/// All nullable properties default to null (no filter applied).
/// </summary>
public partial class ResultsFilterState : ReactiveObject
{
    /// <summary>
    /// Minimum AD (AROI/DD%) - Annual Return Over Investment divided by Drawdown percentage
    /// </summary>
    [Reactive]
    private double? _minAD;

    /// <summary>
    /// Minimum AMWT (Average Minutes per Winning Trade)
    /// </summary>
    [Reactive]
    private double? _minAMWT;

    /// <summary>
    /// Minimum Fitness (net profit)
    /// </summary>
    [Reactive]
    private double? _minFitness;

    /// <summary>
    /// Minimum total number of trades
    /// </summary>
    [Reactive]
    private int? _minTrades;

    /// <summary>
    /// Minimum win rate (0.0 to 1.0)
    /// </summary>
    [Reactive]
    private double? _minWinRate;

    /// <summary>
    /// Maximum drawdown percentage (0.0 to 100.0)
    /// </summary>
    [Reactive]
    private double? _maxDrawdownPercent;

    /// <summary>
    /// Include aborted tests in results (default: true)
    /// </summary>
    [Reactive]
    private bool _includeAborted = true;

    /// <summary>
    /// Reset all filters to their default values (no filtering)
    /// </summary>
    public void Reset()
    {
        MinAD = null;
        MinAMWT = null;
        MinFitness = null;
        MinTrades = null;
        MinWinRate = null;
        MaxDrawdownPercent = null;
        IncludeAborted = true;
    }

    /// <summary>
    /// Check if any filter is currently applied
    /// </summary>
    public bool HasActiveFilters =>
        MinAD.HasValue ||
        MinAMWT.HasValue ||
        MinFitness.HasValue ||
        MinTrades.HasValue ||
        MinWinRate.HasValue ||
        MaxDrawdownPercent.HasValue ||
        !IncludeAborted;
}
