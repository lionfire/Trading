using Orleans;

namespace LionFire.Trading.Grains.Bots;

/// <summary>
/// Metadata from optimization/backtest results, stored alongside a bot's deployed parameters.
/// Used to populate <see cref="BotSignalEvent"/> fields and enrich signals
/// with historical performance context.
/// </summary>
[GenerateSerializer]
[Immutable]
public sealed record BotDeploymentMetadata
{
    /// <summary>
    /// Identifier for the backtest run that produced these results.
    /// </summary>
    [Id(0)]
    public string BacktestId { get; init; } = "";

    /// <summary>
    /// Average Duration of Winning Trades. Controls signal decay rate.
    /// </summary>
    [Id(1)]
    public TimeSpan ADWT { get; init; }

    /// <summary>
    /// Win rate from backtest (0.0 to 1.0).
    /// </summary>
    [Id(2)]
    public double WinRate { get; init; }

    /// <summary>
    /// Profit factor from backtest (gross profit / gross loss).
    /// </summary>
    [Id(3)]
    public double ProfitFactor { get; init; }

    /// <summary>
    /// Average Deviation (AD) score from optimization — the primary fitness metric.
    /// </summary>
    [Id(4)]
    public double AD { get; init; }

    /// <summary>
    /// Tags for diversity scoring in signal aggregation (e.g., "momentum", "mean-reversion", "trend").
    /// </summary>
    [Id(5)]
    public List<string> Tags { get; init; } = [];

    /// <summary>
    /// Average trade duration (all trades, not just winners).
    /// </summary>
    [Id(6)]
    public TimeSpan AverageTradeDuration { get; init; }

    /// <summary>
    /// Average profit per winning trade (as a percentage of entry price).
    /// </summary>
    [Id(7)]
    public double AverageWinPercent { get; init; }

    /// <summary>
    /// Average loss per losing trade (as a percentage of entry price, positive number).
    /// </summary>
    [Id(8)]
    public double AverageLossPercent { get; init; }

    /// <summary>
    /// Total number of trades in the backtest.
    /// </summary>
    [Id(9)]
    public int TotalTrades { get; init; }
}
