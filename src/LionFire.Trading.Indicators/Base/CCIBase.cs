using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using LionFire.Structures;
using System.Numerics;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Base class for CCI (Commodity Channel Index) implementations
/// </summary>
public abstract class CCIBase<TConcrete, TPrice, TOutput> : SingleInputIndicatorBase<TConcrete, PCCI<TPrice, TOutput>, HLC<TPrice>, TOutput>, 
    ICCI<HLC<TPrice>, TOutput>
    where TConcrete : CCIBase<TConcrete, TPrice, TOutput>, IIndicator2<TConcrete, PCCI<TPrice, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Parameters

    protected readonly PCCI<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for CCI calculation
    /// </summary>
    public int Period => Parameters.Period;

    /// <summary>
    /// The constant used in CCI calculation (typically 0.015)
    /// </summary>
    public double Constant => Parameters.Constant;

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period;

    #endregion

    #region Lifecycle

    protected CCIBase(PCCI<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    #endregion

    #region State

    /// <summary>
    /// Current CCI value
    /// </summary>
    public abstract TOutput Value { get; }

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public abstract override bool IsReady { get; }

    #endregion

    #region Static

    /// <summary>
    /// Gets the output slots for the CCI indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "CCI",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the CCI indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PCCI<TPrice, TOutput> p)
        => [new() {
                Name = "CCI",
                ValueType = typeof(TOutput),
            }];

    #endregion
}