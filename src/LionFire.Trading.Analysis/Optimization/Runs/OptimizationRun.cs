using LionFire.Trading.Structures;

namespace LionFire.Trading.Automation.Optimization;

/// <summary>
/// Multi ViewModel
/// </summary>
public class OptimizationRun
{
    public SymbolId SymbolId => SymbolId.Parse(Id?.Symbol);
    public string? Symbol => SymbolId.StandardizedSymbol;
    public string? TimeFrame => Id?.TimeFrame;
    public string? Start => Id?.Start;
    public string? End => Id?.End;
    public double? Days => Id?.Days;
    public int? BacktestsCount => Stats?.BacktestsCount;
    public string TPM => (Stats?.TradesPerMonth.Mean ?? 0.0).ToString("#.0");

    public Histogram? ADHistogram => Stats?.ADHistogram;
    public Histogram? PADHistogram => Stats?.PADHistogram;
    public Histogram? DADHistogram => Stats?.DADHistogram;
    public Histogram? NADHistogram => Stats?.NADHistogram;
    public Histogram? AADHistogram => Stats?.AADHistogram;

    public int? ADCount => Stats?.ADHistogram.AtLeast(1);
    public int? PADCount => Stats?.PADHistogram.AtLeast(1);
    public int? DADCount => Stats?.DADHistogram.AtLeast(1);
    public int? NADCount => Stats?.NADHistogram.AtLeast(1);
    public int? AADCount => Stats?.AADHistogram.AtLeast(1);

    public string? Score => Stats?.Score.ToString("#.00");
    public string? DadScore => Stats?.DadScore.ToString("#.00");
    public string? NadScore => Stats?.NadScore.ToString("#.00");

    #region Model

    public OptimizationRunId Id { get; set; }
    public OptimizationRunBacktests? Backtests { get; set; }

    public OptimizationRunNotes? Notes { get; set; }
    public OptimizationRunStats? Stats { get; set; }

    #endregion
}

