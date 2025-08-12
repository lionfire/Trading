using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using LionFire.Structures;
using System.Numerics;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Base class for Linear Regression implementations
/// </summary>
public abstract class LinearRegressionBase<TConcrete, TPrice, TOutput> : SingleInputIndicatorBase<TConcrete, PLinearRegression<TPrice, TOutput>, TPrice, TOutput>, 
    ILinearRegression<TPrice, TOutput>
    where TConcrete : LinearRegressionBase<TConcrete, TPrice, TOutput>, IIndicator2<TConcrete, PLinearRegression<TPrice, TOutput>, TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Parameters

    protected readonly PLinearRegression<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for Linear Regression calculation
    /// </summary>
    public int Period => Parameters.Period;

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period;

    #endregion

    #region Lifecycle

    protected LinearRegressionBase(PLinearRegression<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    #endregion

    #region State

    /// <summary>
    /// Current regression value (current point on regression line)
    /// </summary>
    public abstract TOutput Value { get; }

    /// <summary>
    /// Current slope of the regression line (rate of change)
    /// </summary>
    public abstract TOutput Slope { get; }

    /// <summary>
    /// Current intercept of the regression line
    /// </summary>
    public abstract TOutput Intercept { get; }

    /// <summary>
    /// Current R-squared value (coefficient of determination)
    /// </summary>
    public abstract TOutput RSquared { get; }

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public abstract override bool IsReady { get; }

    #endregion

    #region Static

    /// <summary>
    /// Gets the output slots for the Linear Regression indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() {
                Name = "Value",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Slope",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Intercept",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "RSquared",
                ValueType = typeof(TOutput),
            }
        ];

    /// <summary>
    /// Gets the output slots for the Linear Regression indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PLinearRegression<TPrice, TOutput> p)
        => [
            new() {
                Name = "Value",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Slope",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Intercept",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "RSquared",
                ValueType = typeof(TOutput),
            }
        ];

    #endregion
}