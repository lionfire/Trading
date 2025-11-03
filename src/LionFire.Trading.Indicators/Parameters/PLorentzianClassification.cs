using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.DataFlow.Indicators;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.Parameters;

/// <summary>
/// Shared parameters for all Lorentzian Classification implementations.
/// An advanced ML-based indicator that uses Lorentzian distance for K-nearest neighbors classification.
/// </summary>
public class PLorentzianClassification<TPrice, TOutput> : IndicatorParameters<OHLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Identity

    [JsonIgnore]
    public override string Key => $"LorentzianClassification(K:{NeighborsCount},L:{LookbackPeriod},N:{NormalizationWindow})";

    #endregion

    #region Parameters

    /// <summary>
    /// The number of nearest neighbors to consider in k-NN classification (default: 8)
    /// </summary>
    [TradingParameter(
        HardValueMin = 1,
        DefaultMin = 3,
        DefaultMax = 50,
        ValueMax = 100,
        HardValueMax = 1000,
        MinStep = 1,
        Step = 1,
        MaxStep = 5,
        OptimizerHints = OptimizationDistributionKind.Period,
        DefaultValue = 8)]
    public int NeighborsCount { get; set; } = 8;

    /// <summary>
    /// The lookback period for storing historical patterns (default: 100)
    /// </summary>
    [TradingParameter(
        HardValueMin = 20,
        DefaultMin = 50,
        DefaultMax = 1000,
        ValueMax = 5000,
        HardValueMax = 100_000,
        DefaultExponent = 1.618,
        MinExponent = 1.0,
        MaxExponent = 2.0,
        MinStep = 10,
        Step = 10,
        MaxStep = 100,
        OptimizerHints = OptimizationDistributionKind.Period,
        DefaultValue = 100)]
    public int LookbackPeriod { get; set; } = 100;

    /// <summary>
    /// The window size for feature normalization (default: 20)
    /// </summary>
    [TradingParameter(
        HardValueMin = 5,
        DefaultMin = 10,
        DefaultMax = 50,
        ValueMax = 200,
        HardValueMax = 1000,
        MinStep = 1,
        Step = 5,
        MaxStep = 10,
        OptimizerHints = OptimizationDistributionKind.Period,
        DefaultValue = 20)]
    public int NormalizationWindow { get; set; } = 20;

    /// <summary>
    /// RSI period for feature extraction (default: 14)
    /// </summary>
    [TradingParameter(
        HardValueMin = 2,
        DefaultMin = 5,
        DefaultMax = 50,
        ValueMax = 100,
        HardValueMax = 200,
        MinStep = 1,
        Step = 1,
        MaxStep = 5,
        OptimizerHints = OptimizationDistributionKind.Period,
        DefaultValue = 14)]
    public int RSIPeriod { get; set; } = 14;

    /// <summary>
    /// CCI period for feature extraction (default: 20)
    /// </summary>
    [TradingParameter(
        HardValueMin = 2,
        DefaultMin = 10,
        DefaultMax = 50,
        ValueMax = 100,
        HardValueMax = 200,
        MinStep = 1,
        Step = 1,
        MaxStep = 5,
        OptimizerHints = OptimizationDistributionKind.Period,
        DefaultValue = 20)]
    public int CCIPeriod { get; set; } = 20;

    /// <summary>
    /// ADX period for feature extraction (default: 14)
    /// </summary>
    [TradingParameter(
        HardValueMin = 2,
        DefaultMin = 5,
        DefaultMax = 50,
        ValueMax = 100,
        HardValueMax = 200,
        MinStep = 1,
        Step = 1,
        MaxStep = 5,
        OptimizerHints = OptimizationDistributionKind.Period,
        DefaultValue = 14)]
    public int ADXPeriod { get; set; } = 14;

    /// <summary>
    /// Minimum confidence required to generate a signal (default: 0.6)
    /// </summary>
    [TradingParameter(
        HardValueMin = 0.0,
        DefaultMin = 0.5,
        DefaultMax = 0.9,
        HardValueMax = 1.0,
        Step = 0.05,
        OptimizePriority = -1,
        DefaultValue = 0.6)]
    public double MinConfidence { get; set; } = 0.6;

    /// <summary>
    /// Number of bars ahead to look for classification labels (default: 5)
    /// This determines how many bars in the future we look to determine if the pattern was bullish/bearish
    /// </summary>
    [TradingParameter(
        HardValueMin = 1,
        DefaultMin = 2,
        DefaultMax = 20,
        ValueMax = 50,
        HardValueMax = 100,
        MinStep = 1,
        Step = 1,
        MaxStep = 5,
        OptimizerHints = OptimizationDistributionKind.Period,
        DefaultValue = 5)]
    public int LabelLookahead { get; set; } = 5;

    /// <summary>
    /// Threshold for labeling patterns as bullish/bearish based on price change (default: 0.01 = 1%)
    /// </summary>
    [TradingParameter(
        HardValueMin = 0.0001,
        DefaultMin = 0.005,
        DefaultMax = 0.05,
        HardValueMax = 0.2,
        Step = 0.001,
        OptimizePriority = -1,
        DefaultValue = 0.01)]
    public double LabelThreshold { get; set; } = 0.01;

    /// <summary>
    /// Implementation selection hint for runtime selection
    /// </summary>
    [TradingParameter(OptimizePriority = -10)]
    public ImplementationHint ImplementationHint { get; set; } = ImplementationHint.Auto;

    #endregion

    #region Type Info

    public static IReadOnlyList<InputSlot> GetInputSlots()
        => [new InputSlot()
        {
            Name = "OHLC",
            ValueType = typeof(OHLC<TPrice>),
            Aspects = DataPointAspect.Open | DataPointAspect.High | DataPointAspect.Low | DataPointAspect.Close,
            DefaultSource = 0,
        }];

    #endregion

    #region Inputs

    public int LookbackForInputSlot(InputSlot inputSlot) => Math.Max(LookbackPeriod, Math.Max(Math.Max(RSIPeriod, CCIPeriod), ADXPeriod) + LabelLookahead);

    #endregion

    #region Validation

    /// <summary>
    /// Validates the parameters for consistency
    /// </summary>
    public void Validate()
    {
        if (NeighborsCount >= LookbackPeriod)
            throw new ArgumentException($"{nameof(NeighborsCount)} ({NeighborsCount}) must be less than {nameof(LookbackPeriod)} ({LookbackPeriod})");

        if (NormalizationWindow > LookbackPeriod)
            throw new ArgumentException($"{nameof(NormalizationWindow)} ({NormalizationWindow}) should not exceed {nameof(LookbackPeriod)} ({LookbackPeriod})");

        if (MinConfidence < 0.0 || MinConfidence > 1.0)
            throw new ArgumentException($"{nameof(MinConfidence)} ({MinConfidence}) must be between 0.0 and 1.0");

        if (LabelThreshold <= 0.0)
            throw new ArgumentException($"{nameof(LabelThreshold)} ({LabelThreshold}) must be positive");

        if (LabelLookahead < 1)
            throw new ArgumentException($"{nameof(LabelLookahead)} ({LabelLookahead}) must be at least 1");
    }

    #endregion
}