using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;
using System.Reflection;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Klinger Oscillator indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class KlingerOscillator
{
    /// <summary>
    /// Creates a Klinger Oscillator indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TInput">The input HLCV data type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The Klinger Oscillator parameters</param>
    /// <returns>A Klinger Oscillator indicator instance</returns>
    public static IKlingerOscillator<TInput, TOutput> Create<TInput, TOutput>(PKlingerOscillator<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.ImplementationHint switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new KlingerOscillator_FP<TInput, TOutput>(parameters),
            ImplementationHint.Optimized => new KlingerOscillator_FP<TInput, TOutput>(parameters), // FP is already optimized
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a Klinger Oscillator indicator with default parameters.
    /// </summary>
    /// <typeparam name="TInput">The input HLCV data type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="fastPeriod">The fast period for EMA calculation (default: 34)</param>
    /// <param name="slowPeriod">The slow period for EMA calculation (default: 55)</param>
    /// <param name="signalPeriod">The signal period for EMA calculation of the Klinger line (default: 13)</param>
    /// <returns>A Klinger Oscillator indicator instance</returns>
    public static IKlingerOscillator<TInput, TOutput> Create<TInput, TOutput>(int fastPeriod = 34, int slowPeriod = 55, int signalPeriod = 13)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PKlingerOscillator<TInput, TOutput> 
        { 
            FastPeriod = fastPeriod,
            SlowPeriod = slowPeriod,
            SignalPeriod = signalPeriod
        };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// Falls back to first-party implementation since QuantConnect doesn't have Klinger Oscillator.
    /// </summary>
    private static IKlingerOscillator<TInput, TOutput> CreateQuantConnectImplementation<TInput, TOutput>(PKlingerOscillator<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        try
        {
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var klingerQcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.KlingerOscillator_QC`2");
            
            if (klingerQcType != null)
            {
                // Make the generic type
                var genericType = klingerQcType.MakeGenericType(typeof(TInput), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IKlingerOscillator<TInput, TOutput> klinger)
                {
                    return klinger;
                }
            }
        }
        catch
        {
            // Fall back to first-party implementation if QuantConnect is not available or doesn't have Klinger
        }
        
        // Fallback to first-party implementation (QuantConnect doesn't have Klinger Oscillator)
        return new KlingerOscillator_FP<TInput, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IKlingerOscillator<TInput, TOutput> SelectBestImplementation<TInput, TOutput>(PKlingerOscillator<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        // Decision logic for selecting implementation:
        // - Since QuantConnect doesn't have Klinger Oscillator, we always use first-party
        // - The first-party implementation is optimized with efficient EMA calculations
        // - Default to first-party for its efficiency and complete feature set

        return new KlingerOscillator_FP<TInput, TOutput>(parameters);
    }

    /// <summary>
    /// Convenience method to create a Klinger Oscillator indicator for double values.
    /// </summary>
    /// <param name="fastPeriod">The fast period for EMA calculation (default: 34)</param>
    /// <param name="slowPeriod">The slow period for EMA calculation (default: 55)</param>
    /// <param name="signalPeriod">The signal period for EMA calculation of the Klinger line (default: 13)</param>
    /// <returns>A Klinger Oscillator indicator instance for double values</returns>
    public static IKlingerOscillator<double, double> CreateDouble(int fastPeriod = 34, int slowPeriod = 55, int signalPeriod = 13)
    {
        return Create<double, double>(fastPeriod, slowPeriod, signalPeriod);
    }

    /// <summary>
    /// Convenience method to create a Klinger Oscillator indicator for decimal values.
    /// </summary>
    /// <param name="fastPeriod">The fast period for EMA calculation (default: 34)</param>
    /// <param name="slowPeriod">The slow period for EMA calculation (default: 55)</param>
    /// <param name="signalPeriod">The signal period for EMA calculation of the Klinger line (default: 13)</param>
    /// <returns>A Klinger Oscillator indicator instance for decimal values</returns>
    public static IKlingerOscillator<decimal, decimal> CreateDecimal(int fastPeriod = 34, int slowPeriod = 55, int signalPeriod = 13)
    {
        return Create<decimal, decimal>(fastPeriod, slowPeriod, signalPeriod);
    }

    /// <summary>
    /// Convenience method to create a Klinger Oscillator indicator for float values.
    /// </summary>
    /// <param name="fastPeriod">The fast period for EMA calculation (default: 34)</param>
    /// <param name="slowPeriod">The slow period for EMA calculation (default: 55)</param>
    /// <param name="signalPeriod">The signal period for EMA calculation of the Klinger line (default: 13)</param>
    /// <returns>A Klinger Oscillator indicator instance for float values</returns>
    public static IKlingerOscillator<float, float> CreateFloat(int fastPeriod = 34, int slowPeriod = 55, int signalPeriod = 13)
    {
        return Create<float, float>(fastPeriod, slowPeriod, signalPeriod);
    }
}