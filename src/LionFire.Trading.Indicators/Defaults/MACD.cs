using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;
using System.Reflection;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default MACD (Moving Average Convergence Divergence) indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class MACD
{
    /// <summary>
    /// Creates a MACD indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The MACD parameters</param>
    /// <returns>A MACD indicator instance</returns>
    public static IMACD<TPrice, TOutput> Create<TPrice, TOutput>(PMACD<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.ImplementationHint switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new MACD_FP<TPrice, TOutput>(parameters),
            ImplementationHint.Optimized => new MACD_FP<TPrice, TOutput>(parameters), // FP is already optimized with EMA calculations
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a MACD indicator with default parameters.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="fastPeriod">The fast period for EMA calculation (default: 12)</param>
    /// <param name="slowPeriod">The slow period for EMA calculation (default: 26)</param>
    /// <param name="signalPeriod">The signal period for EMA calculation of the MACD line (default: 9)</param>
    /// <returns>A MACD indicator instance</returns>
    public static IMACD<TPrice, TOutput> Create<TPrice, TOutput>(int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PMACD<TPrice, TOutput> 
        { 
            FastPeriod = fastPeriod,
            SlowPeriod = slowPeriod,
            SignalPeriod = signalPeriod
        };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IMACD<TPrice, TOutput> CreateQuantConnectImplementation<TPrice, TOutput>(PMACD<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        try
        {
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var macdQcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.MACD_QC`2");
            
            if (macdQcType != null)
            {
                // Make the generic type
                var genericType = macdQcType.MakeGenericType(typeof(TPrice), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IMACD<TPrice, TOutput> macd)
                {
                    return macd;
                }
            }
        }
        catch
        {
            // Fall back to first-party implementation if QuantConnect is not available
        }
        
        // Fallback to first-party implementation
        return new MACD_FP<TPrice, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IMACD<TPrice, TOutput> SelectBestImplementation<TPrice, TOutput>(PMACD<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        // Decision logic for selecting implementation:
        // - For most cases, the first-party implementation with optimized EMA calculations is very efficient
        // - For high-frequency scenarios or when QuantConnect is already loaded, consider QuantConnect
        // - Default to first-party for its simplicity and efficiency

        // Check if QuantConnect assembly is already loaded (to avoid loading it unnecessarily)
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);

        if (quantConnectLoaded && (parameters.FastPeriod > 50 || parameters.SlowPeriod > 100))
        {
            // Use QuantConnect for very large periods when it's already loaded
            return CreateQuantConnectImplementation(parameters);
        }
        else
        {
            // Use first-party implementation for efficiency and optimized EMA calculations
            return new MACD_FP<TPrice, TOutput>(parameters);
        }
    }

    /// <summary>
    /// Convenience method to create a MACD indicator for double values.
    /// </summary>
    /// <param name="fastPeriod">The fast period for EMA calculation (default: 12)</param>
    /// <param name="slowPeriod">The slow period for EMA calculation (default: 26)</param>
    /// <param name="signalPeriod">The signal period for EMA calculation of the MACD line (default: 9)</param>
    /// <returns>A MACD indicator instance for double values</returns>
    public static IMACD<double, double> CreateDouble(int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
    {
        return Create<double, double>(fastPeriod, slowPeriod, signalPeriod);
    }

    /// <summary>
    /// Convenience method to create a MACD indicator for decimal values.
    /// </summary>
    /// <param name="fastPeriod">The fast period for EMA calculation (default: 12)</param>
    /// <param name="slowPeriod">The slow period for EMA calculation (default: 26)</param>
    /// <param name="signalPeriod">The signal period for EMA calculation of the MACD line (default: 9)</param>
    /// <returns>A MACD indicator instance for decimal values</returns>
    public static IMACD<decimal, decimal> CreateDecimal(int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
    {
        return Create<decimal, decimal>(fastPeriod, slowPeriod, signalPeriod);
    }

    /// <summary>
    /// Convenience method to create a MACD indicator for float values.
    /// </summary>
    /// <param name="fastPeriod">The fast period for EMA calculation (default: 12)</param>
    /// <param name="slowPeriod">The slow period for EMA calculation (default: 26)</param>
    /// <param name="signalPeriod">The signal period for EMA calculation of the MACD line (default: 9)</param>
    /// <returns>A MACD indicator instance for float values</returns>
    public static IMACD<float, float> CreateFloat(int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
    {
        return Create<float, float>(fastPeriod, slowPeriod, signalPeriod);
    }
}