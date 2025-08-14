using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.DataFlow.Indicators;
using System.Numerics;
using System.Reflection;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Choppiness Index indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class ChoppinessIndex
{
    /// <summary>
    /// Creates a Choppiness Index indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TInput">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The Choppiness Index parameters</param>
    /// <returns>A Choppiness Index indicator instance</returns>
    public static IChoppinessIndex<TInput, TOutput> Create<TInput, TOutput>(PChoppinessIndex<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.PreferredImplementation switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new ChoppinessIndex_FP<TInput, TOutput>(parameters),
            ImplementationHint.Optimized => new ChoppinessIndex_FP<TInput, TOutput>(parameters), // FP is already optimized with circular buffers
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a Choppiness Index indicator with default parameters.
    /// </summary>
    /// <typeparam name="TInput">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="period">The period for the Choppiness Index calculation (default: 14)</param>
    /// <param name="choppyThreshold">The choppy threshold (default: 61.8)</param>
    /// <param name="trendingThreshold">The trending threshold (default: 38.2)</param>
    /// <returns>A Choppiness Index indicator instance</returns>
    public static IChoppinessIndex<TInput, TOutput> Create<TInput, TOutput>(
        int period = 14, 
        double choppyThreshold = 61.8, 
        double trendingThreshold = 38.2)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PChoppinessIndex<TInput, TOutput> 
        { 
            Period = period,
            ChoppyThreshold = TOutput.CreateChecked(choppyThreshold),
            TrendingThreshold = TOutput.CreateChecked(trendingThreshold)
        };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// Since there's no QuantConnect ChoppinessIndex, this falls back to first-party implementation.
    /// </summary>
    private static IChoppinessIndex<TInput, TOutput> CreateQuantConnectImplementation<TInput, TOutput>(PChoppinessIndex<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        try
        {
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var choppinessqcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.ChoppinessIndex_QC`2");
            
            if (choppinessqcType != null)
            {
                // Make the generic type
                var genericType = choppinessqcType.MakeGenericType(typeof(TInput), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IChoppinessIndex<TInput, TOutput> choppinessIndex)
                {
                    return choppinessIndex;
                }
            }
        }
        catch
        {
            // Fall back to first-party implementation if QuantConnect is not available
        }
        
        // Fallback to first-party implementation
        return new ChoppinessIndex_FP<TInput, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IChoppinessIndex<TInput, TOutput> SelectBestImplementation<TInput, TOutput>(PChoppinessIndex<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        // Decision logic for selecting implementation:
        // - The first-party implementation with circular buffers is efficient for all periods
        // - Since ChoppinessIndex is not commonly implemented in other libraries, default to first-party
        // - First-party implementation provides accurate Choppiness Index calculation with proper true range handling

        // Always use first-party implementation as it's specifically optimized for Choppiness Index calculation
        return new ChoppinessIndex_FP<TInput, TOutput>(parameters);
    }

    /// <summary>
    /// Convenience method to create a Choppiness Index indicator for double values.
    /// </summary>
    /// <param name="period">The period for the Choppiness Index calculation</param>
    /// <param name="choppyThreshold">The choppy threshold (default: 61.8)</param>
    /// <param name="trendingThreshold">The trending threshold (default: 38.2)</param>
    /// <returns>A Choppiness Index indicator instance for double values</returns>
    public static IChoppinessIndex<double, double> CreateDouble(
        int period = 14, 
        double choppyThreshold = 61.8, 
        double trendingThreshold = 38.2)
    {
        return Create<double, double>(period, choppyThreshold, trendingThreshold);
    }

    /// <summary>
    /// Convenience method to create a Choppiness Index indicator for decimal values.
    /// </summary>
    /// <param name="period">The period for the Choppiness Index calculation</param>
    /// <param name="choppyThreshold">The choppy threshold (default: 61.8)</param>
    /// <param name="trendingThreshold">The trending threshold (default: 38.2)</param>
    /// <returns>A Choppiness Index indicator instance for decimal values</returns>
    public static IChoppinessIndex<decimal, decimal> CreateDecimal(
        int period = 14, 
        double choppyThreshold = 61.8, 
        double trendingThreshold = 38.2)
    {
        return Create<decimal, decimal>(period, choppyThreshold, trendingThreshold);
    }

    /// <summary>
    /// Convenience method to create a Choppiness Index indicator for float values.
    /// </summary>
    /// <param name="period">The period for the Choppiness Index calculation</param>
    /// <param name="choppyThreshold">The choppy threshold (default: 61.8)</param>
    /// <param name="trendingThreshold">The trending threshold (default: 38.2)</param>
    /// <returns>A Choppiness Index indicator instance for float values</returns>
    public static IChoppinessIndex<float, float> CreateFloat(
        int period = 14, 
        double choppyThreshold = 61.8, 
        double trendingThreshold = 38.2)
    {
        return Create<float, float>(period, choppyThreshold, trendingThreshold);
    }
}