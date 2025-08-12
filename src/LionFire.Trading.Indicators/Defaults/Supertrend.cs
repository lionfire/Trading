using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using System.Numerics;
using System.Reflection;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Supertrend indicator factory.
/// Automatically selects the best implementation based on the PreferredImplementation parameter.
/// </summary>
public static class Supertrend
{
    /// <summary>
    /// Creates a Supertrend indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The Supertrend parameters</param>
    /// <returns>A Supertrend indicator instance</returns>
    public static ISupertrend<TPrice, TOutput> Create<TPrice, TOutput>(PSupertrend<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.PreferredImplementation switch
        {
            ImplementationHint.QuantConnect => new Supertrend_QC<TPrice, TOutput>(parameters),
            ImplementationHint.FirstParty => new Supertrend_FP<TPrice, TOutput>(parameters),
            ImplementationHint.Optimized => SelectOptimizedImplementation(parameters),
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a Supertrend indicator with default parameters.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="atrPeriod">The period for ATR calculation (default: 10)</param>
    /// <param name="multiplier">The multiplier for ATR bands (default: 3.0)</param>
    /// <returns>A Supertrend indicator instance</returns>
    public static ISupertrend<TPrice, TOutput> Create<TPrice, TOutput>(int atrPeriod = 10, TOutput? multiplier = null)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PSupertrend<TPrice, TOutput> 
        { 
            AtrPeriod = atrPeriod,
            Multiplier = multiplier ?? TOutput.CreateChecked(3.0)
        };
        return Create(parameters);
    }

    /// <summary>
    /// Selects the optimized implementation based on the specific requirements.
    /// </summary>
    private static ISupertrend<TPrice, TOutput> SelectOptimizedImplementation<TPrice, TOutput>(PSupertrend<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        // For Supertrend, the first-party implementation is typically more optimized
        // because it implements ATR internally with Wilder's smoothing and has efficient
        // trend persistence logic optimized for streaming updates
        
        // Check if QuantConnect is already loaded for compatibility reasons
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("QuantConnect") == true);

        if (quantConnectLoaded)
        {
            // Use QuantConnect if it's already loaded for ecosystem consistency
            return new Supertrend_QC<TPrice, TOutput>(parameters);
        }
        else
        {
            // Use first-party implementation for better performance
            return new Supertrend_FP<TPrice, TOutput>(parameters);
        }
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static ISupertrend<TPrice, TOutput> SelectBestImplementation<TPrice, TOutput>(PSupertrend<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        // Decision logic for selecting implementation:
        // - Supertrend is computationally intensive with ATR + trend logic
        // - First-party implementation has optimized ATR calculation and streaming updates
        // - QuantConnect provides battle-tested ATR but has TradeBar overhead
        // - Default to first-party for better performance in most scenarios

        // Check if QuantConnect assembly is already loaded (to avoid loading it unnecessarily)
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("QuantConnect") == true);

        if (quantConnectLoaded)
        {
            // Use QuantConnect if it's already loaded for consistency
            return new Supertrend_QC<TPrice, TOutput>(parameters);
        }
        else
        {
            // Use first-party implementation for efficiency and optimized ATR
            return new Supertrend_FP<TPrice, TOutput>(parameters);
        }
    }

    /// <summary>
    /// Convenience method to create a Supertrend indicator for double values.
    /// </summary>
    /// <param name="atrPeriod">The period for ATR calculation</param>
    /// <param name="multiplier">The multiplier for ATR bands</param>
    /// <returns>A Supertrend indicator instance for double values</returns>
    public static ISupertrend<double, double> CreateDouble(int atrPeriod = 10, double multiplier = 3.0)
    {
        return Create<double, double>(atrPeriod, multiplier);
    }

    /// <summary>
    /// Convenience method to create a Supertrend indicator for decimal values.
    /// </summary>
    /// <param name="atrPeriod">The period for ATR calculation</param>
    /// <param name="multiplier">The multiplier for ATR bands</param>
    /// <returns>A Supertrend indicator instance for decimal values</returns>
    public static ISupertrend<decimal, decimal> CreateDecimal(int atrPeriod = 10, decimal multiplier = 3.0m)
    {
        return Create<decimal, decimal>(atrPeriod, multiplier);
    }

    /// <summary>
    /// Convenience method to create a Supertrend indicator for float values.
    /// </summary>
    /// <param name="atrPeriod">The period for ATR calculation</param>
    /// <param name="multiplier">The multiplier for ATR bands</param>
    /// <returns>A Supertrend indicator instance for float values</returns>
    public static ISupertrend<float, float> CreateFloat(int atrPeriod = 10, float multiplier = 3.0f)
    {
        return Create<float, float>(atrPeriod, multiplier);
    }
}