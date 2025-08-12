using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Linear Regression indicator interface.
/// </summary>
/// <remarks>
/// Available implementations:
/// - LinearRegression_QC: QuantConnect implementation (default, stable)
/// - LinearRegression_FP: First-party implementation (custom features, optimized with circular buffer)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface ILinearRegression<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The period used for Linear Regression calculation
    /// </summary>
    int Period { get; }
    
    /// <summary>
    /// Current regression value (current point on regression line)
    /// </summary>
    TOutput Value { get; }
    
    /// <summary>
    /// Current slope of the regression line (rate of change)
    /// </summary>
    TOutput Slope { get; }
    
    /// <summary>
    /// Current intercept of the regression line
    /// </summary>
    TOutput Intercept { get; }
    
    /// <summary>
    /// Current R-squared value (coefficient of determination, optional)
    /// </summary>
    TOutput RSquared { get; }
}