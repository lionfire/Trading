using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using LionFire.Structures;
using System.Numerics;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Base class for Ichimoku Cloud implementations
/// </summary>
public abstract class IchimokuCloudBase<TConcrete, TPrice, TOutput> : SingleInputIndicatorBase<TConcrete, PIchimokuCloud<TPrice, TOutput>, HLC<TPrice>, TOutput>, 
    IIchimokuCloud<HLC<TPrice>, TOutput>
    where TConcrete : IchimokuCloudBase<TConcrete, TPrice, TOutput>, IIndicator2<TConcrete, PIchimokuCloud<TPrice, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Parameters

    protected readonly PIchimokuCloud<TPrice, TOutput> Parameters;

    /// <summary>
    /// Conversion Line Period (Tenkan-sen)
    /// </summary>
    public int ConversionLinePeriod => Parameters.ConversionLinePeriod;

    /// <summary>
    /// Base Line Period (Kijun-sen)
    /// </summary>
    public int BaseLinePeriod => Parameters.BaseLinePeriod;

    /// <summary>
    /// Leading Span B Period (Senkou Span B)
    /// </summary>
    public int LeadingSpanBPeriod => Parameters.LeadingSpanBPeriod;

    /// <summary>
    /// Displacement (periods ahead for leading spans and behind for lagging span)
    /// </summary>
    public int Displacement => Parameters.Displacement;

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Math.Max(LeadingSpanBPeriod, BaseLinePeriod) + Displacement;

    #endregion

    #region Lifecycle

    protected IchimokuCloudBase(PIchimokuCloud<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        Parameters.Validate();
    }

    #endregion

    #region State

    /// <summary>
    /// Tenkan-sen (Conversion Line): (9-period high + 9-period low) / 2
    /// </summary>
    public abstract TOutput TenkanSen { get; }

    /// <summary>
    /// Kijun-sen (Base Line): (26-period high + 26-period low) / 2
    /// </summary>
    public abstract TOutput KijunSen { get; }

    /// <summary>
    /// Senkou Span A (Leading Span A): (Tenkan + Kijun) / 2, plotted 26 periods ahead
    /// </summary>
    public abstract TOutput SenkouSpanA { get; }

    /// <summary>
    /// Senkou Span B (Leading Span B): (52-period high + 52-period low) / 2, plotted 26 periods ahead
    /// </summary>
    public abstract TOutput SenkouSpanB { get; }

    /// <summary>
    /// Chikou Span (Lagging Span): Close price plotted 26 periods behind
    /// </summary>
    public abstract TOutput ChikouSpan { get; }

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public abstract override bool IsReady { get; }

    #endregion

    #region Cloud Analysis

    /// <summary>
    /// Determines if the cloud is bullish (Senkou Span A > Senkou Span B)
    /// </summary>
    public virtual bool IsBullishCloud 
    { 
        get
        {
            if (!IsReady) return false;
            return SenkouSpanA > SenkouSpanB;
        }
    }

    /// <summary>
    /// Determines if the cloud is bearish (Senkou Span A < Senkou Span B)
    /// </summary>
    public virtual bool IsBearishCloud 
    { 
        get
        {
            if (!IsReady) return false;
            return SenkouSpanA < SenkouSpanB;
        }
    }

    /// <summary>
    /// Gets the thickness of the cloud (absolute difference between Senkou Spans)
    /// </summary>
    public virtual TOutput CloudThickness 
    { 
        get
        {
            if (!IsReady) return default(TOutput)!;
            
            var diff = SenkouSpanA - SenkouSpanB;
            return diff >= TOutput.Zero ? diff : -diff; // Absolute value
        }
    }

    #endregion

    #region Static

    /// <summary>
    /// Gets the output slots for the Ichimoku Cloud indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() { Name = "TenkanSen", ValueType = typeof(TOutput) },
            new() { Name = "KijunSen", ValueType = typeof(TOutput) },
            new() { Name = "SenkouSpanA", ValueType = typeof(TOutput) },
            new() { Name = "SenkouSpanB", ValueType = typeof(TOutput) },
            new() { Name = "ChikouSpan", ValueType = typeof(TOutput) }
        ];

    /// <summary>
    /// Gets the output slots for the Ichimoku Cloud indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PIchimokuCloud<TPrice, TOutput> p)
        => [
            new() { Name = "TenkanSen", ValueType = typeof(TOutput) },
            new() { Name = "KijunSen", ValueType = typeof(TOutput) },
            new() { Name = "SenkouSpanA", ValueType = typeof(TOutput) },
            new() { Name = "SenkouSpanB", ValueType = typeof(TOutput) },
            new() { Name = "ChikouSpan", ValueType = typeof(TOutput) }
        ];

    #endregion
}