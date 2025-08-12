using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using LionFire.Structures;
using System.Numerics;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Base class for Heikin-Ashi (Average Bar) implementations
/// </summary>
public abstract class HeikinAshiBase<TConcrete, TInput, TOutput> : SingleInputIndicatorBase<TConcrete, PHeikinAshi<TInput, TOutput>, TInput, TOutput>, 
    IHeikinAshi<TInput, TOutput>
    where TConcrete : HeikinAshiBase<TConcrete, TInput, TOutput>, IIndicator2<TConcrete, PHeikinAshi<TInput, TOutput>, TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Parameters

    protected readonly PHeikinAshi<TInput, TOutput> Parameters;

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => 1; // Need previous candle to calculate

    /// <summary>
    /// Doji threshold for determining if open and close are approximately equal
    /// </summary>
    public double DojiThreshold => Parameters.DojiThreshold;

    #endregion

    #region Lifecycle

    protected HeikinAshiBase(PHeikinAshi<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    #endregion

    #region State

    /// <summary>
    /// Current Heikin-Ashi Open value
    /// </summary>
    public abstract TOutput HA_Open { get; }
    
    /// <summary>
    /// Current Heikin-Ashi High value
    /// </summary>
    public abstract TOutput HA_High { get; }
    
    /// <summary>
    /// Current Heikin-Ashi Low value
    /// </summary>
    public abstract TOutput HA_Low { get; }
    
    /// <summary>
    /// Current Heikin-Ashi Close value
    /// </summary>
    public abstract TOutput HA_Close { get; }

    /// <summary>
    /// Indicates if the current Heikin-Ashi candle is bullish (close > open)
    /// </summary>
    public virtual bool IsBullish => IsReady && HA_Close > HA_Open;
    
    /// <summary>
    /// Indicates if the current Heikin-Ashi candle is bearish (close < open)
    /// </summary>
    public virtual bool IsBearish => IsReady && HA_Close < HA_Open;
    
    /// <summary>
    /// Indicates if the current Heikin-Ashi candle is a doji (close ~= open)
    /// </summary>
    public virtual bool IsDoji
    {
        get
        {
            if (!IsReady) return false;
            var diff = TOutput.Abs(HA_Close - HA_Open);
            var avg = (HA_Close + HA_Open) / TOutput.CreateChecked(2);
            var threshold = avg * TOutput.CreateChecked(DojiThreshold);
            return diff <= threshold;
        }
    }
    
    /// <summary>
    /// Provides trend strength indication:
    /// 2 = Strong bullish, 1 = Weak bullish, 0 = Neutral/Doji, -1 = Weak bearish, -2 = Strong bearish
    /// </summary>
    public virtual int TrendStrength
    {
        get
        {
            if (!IsReady) return 0;
            if (IsDoji) return 0;
            
            var bodySize = TOutput.Abs(HA_Close - HA_Open);
            var totalRange = HA_High - HA_Low;
            
            if (totalRange == TOutput.Zero) return 0;
            
            var bodyRatio = bodySize / totalRange;
            var strongThreshold = TOutput.CreateChecked(0.7); // Body is 70% of total range
            
            if (IsBullish)
            {
                return bodyRatio > strongThreshold ? 2 : 1;
            }
            else if (IsBearish)
            {
                return bodyRatio > strongThreshold ? -2 : -1;
            }
            
            return 0;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public abstract override bool IsReady { get; }

    #endregion

    #region Static

    /// <summary>
    /// Gets the output slots for the Heikin-Ashi indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() { Name = "HA_Open", ValueType = typeof(TOutput) },
            new() { Name = "HA_High", ValueType = typeof(TOutput) },
            new() { Name = "HA_Low", ValueType = typeof(TOutput) },
            new() { Name = "HA_Close", ValueType = typeof(TOutput) }
        ];

    /// <summary>
    /// Gets the output slots for the Heikin-Ashi indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PHeikinAshi<TInput, TOutput> p)
        => [
            new() { Name = "HA_Open", ValueType = typeof(TOutput) },
            new() { Name = "HA_High", ValueType = typeof(TOutput) },
            new() { Name = "HA_Low", ValueType = typeof(TOutput) },
            new() { Name = "HA_Close", ValueType = typeof(TOutput) }
        ];

    #endregion
}