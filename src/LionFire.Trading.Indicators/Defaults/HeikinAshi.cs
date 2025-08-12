using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Heikin-Ashi (Average Bar) indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class HeikinAshi
{
    /// <summary>
    /// Creates a Heikin-Ashi indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TInput">The input OHLC data type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The Heikin-Ashi parameters</param>
    /// <returns>A Heikin-Ashi indicator instance</returns>
    public static IHeikinAshi<TInput, TOutput> Create<TInput, TOutput>(PHeikinAshi<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.ImplementationHint switch
        {
            ImplementationHint.FirstParty => new HeikinAshi_FP<TInput, TOutput>(parameters),
            ImplementationHint.Optimized => new HeikinAshi_FP<TInput, TOutput>(parameters), // FP is already optimized
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a Heikin-Ashi indicator with default parameters.
    /// </summary>
    /// <typeparam name="TInput">The input OHLC data type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="dojiThreshold">The doji threshold for determining if open and close are approximately equal (default: 0.001)</param>
    /// <returns>A Heikin-Ashi indicator instance</returns>
    public static IHeikinAshi<TInput, TOutput> Create<TInput, TOutput>(double dojiThreshold = 0.001)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PHeikinAshi<TInput, TOutput> { DojiThreshold = dojiThreshold };
        return Create(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IHeikinAshi<TInput, TOutput> SelectBestImplementation<TInput, TOutput>(PHeikinAshi<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        // Heikin-Ashi is a relatively simple indicator, so the first-party implementation
        // is efficient and provides all necessary functionality
        return new HeikinAshi_FP<TInput, TOutput>(parameters);
    }

    /// <summary>
    /// Convenience method to create a Heikin-Ashi indicator for double OHLC values.
    /// </summary>
    /// <param name="dojiThreshold">The doji threshold for determining if open and close are approximately equal</param>
    /// <returns>A Heikin-Ashi indicator instance for double values</returns>
    public static IHeikinAshi<double, double> CreateDouble(double dojiThreshold = 0.001)
    {
        return Create<double, double>(dojiThreshold);
    }

    /// <summary>
    /// Convenience method to create a Heikin-Ashi indicator for decimal OHLC values.
    /// </summary>
    /// <param name="dojiThreshold">The doji threshold for determining if open and close are approximately equal</param>
    /// <returns>A Heikin-Ashi indicator instance for decimal values</returns>
    public static IHeikinAshi<decimal, decimal> CreateDecimal(double dojiThreshold = 0.001)
    {
        return Create<decimal, decimal>(dojiThreshold);
    }

    /// <summary>
    /// Convenience method to create a Heikin-Ashi indicator for float OHLC values.
    /// </summary>
    /// <param name="dojiThreshold">The doji threshold for determining if open and close are approximately equal</param>
    /// <returns>A Heikin-Ashi indicator instance for float values</returns>
    public static IHeikinAshi<float, float> CreateFloat(double dojiThreshold = 0.001)
    {
        return Create<float, float>(dojiThreshold);
    }

    /// <summary>
    /// Convenience method to create a Heikin-Ashi indicator that works with any OHLC bar type.
    /// This is useful when working with IKline or similar bar structures.
    /// </summary>
    /// <typeparam name="TBar">The bar type (must have OHLC properties)</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="dojiThreshold">The doji threshold for determining if open and close are approximately equal</param>
    /// <returns>A Heikin-Ashi indicator instance that accepts bar structures</returns>
    public static IHeikinAshi<TBar, TOutput> CreateForBars<TBar, TOutput>(double dojiThreshold = 0.001)
        where TBar : struct
        where TOutput : struct, INumber<TOutput>
    {
        return Create<TBar, TOutput>(dojiThreshold);
    }
}