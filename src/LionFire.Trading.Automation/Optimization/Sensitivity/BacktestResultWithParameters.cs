namespace LionFire.Trading.Automation.Optimization.Sensitivity;

/// <summary>
/// A backtest result entry with extracted parameter values.
/// Extends the raw journal entry with a dictionary of parameter name → value.
/// </summary>
public class BacktestResultWithParameters
{
    /// <summary>
    /// The raw journal entry (AD, Wins, Losses, etc.).
    /// </summary>
    public required BacktestBatchJournalEntry Entry { get; init; }

    /// <summary>
    /// Extracted parameter values keyed by parameter name (e.g., "P.StopLossATRMultiplier" → 2.5).
    /// Only numeric parameters are included.
    /// </summary>
    public required Dictionary<string, double> ParameterValues { get; init; }

    /// <summary>
    /// Total trade count (Wins + Losses + Breakevens).
    /// </summary>
    public int TotalTrades => Entry.TotalTrades;

    /// <summary>
    /// Confidence weight based on trade count.
    /// </summary>
    public double Confidence { get; init; }
}
