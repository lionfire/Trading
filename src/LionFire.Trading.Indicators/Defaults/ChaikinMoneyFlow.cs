using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.DataFlow.Indicators;
//using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading;
using System.Numerics;
using System.Reflection;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Chaikin Money Flow (CMF) indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class ChaikinMoneyFlow
{
    /// <summary>
    /// Creates a CMF indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TInput">The input data type (should have High, Low, Close, and Volume properties)</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The CMF parameters</param>
    /// <returns>A CMF indicator instance</returns>
    public static IChaikinMoneyFlow<TInput, TOutput> Create<TInput, TOutput>(PChaikinMoneyFlow<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.PreferredImplementation switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new ChaikinMoneyFlow_FP<TInput, TOutput>(parameters),
            ImplementationHint.Optimized => new ChaikinMoneyFlow_FP<TInput, TOutput>(parameters), // FP is already optimized
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a CMF indicator with default parameters (period = 21).
    /// </summary>
    /// <typeparam name="TInput">The input data type (should have High, Low, Close, and Volume properties)</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <returns>A CMF indicator instance</returns>
    public static IChaikinMoneyFlow<TInput, TOutput> Create<TInput, TOutput>()
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PChaikinMoneyFlow<TInput, TOutput>();
        return Create(parameters);
    }

    /// <summary>
    /// Creates a CMF indicator with the specified period.
    /// </summary>
    /// <typeparam name="TInput">The input data type (should have High, Low, Close, and Volume properties)</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="period">The period for CMF calculation</param>
    /// <returns>A CMF indicator instance</returns>
    public static IChaikinMoneyFlow<TInput, TOutput> Create<TInput, TOutput>(int period)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PChaikinMoneyFlow<TInput, TOutput> { Period = period };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IChaikinMoneyFlow<TInput, TOutput> CreateQuantConnectImplementation<TInput, TOutput>(PChaikinMoneyFlow<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        try
        {
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var cmfQcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.ChaikinMoneyFlow_QC`2");
            
            if (cmfQcType != null)
            {
                // Make the generic type
                var genericType = cmfQcType.MakeGenericType(typeof(TInput), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IChaikinMoneyFlow<TInput, TOutput> cmf)
                {
                    return cmf;
                }
            }
        }
        catch
        {
            // Fall back to first-party implementation if QuantConnect is not available
        }
        
        // Fallback to first-party implementation
        return new ChaikinMoneyFlow_FP<TInput, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IChaikinMoneyFlow<TInput, TOutput> SelectBestImplementation<TInput, TOutput>(PChaikinMoneyFlow<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        // Decision logic for selecting implementation:
        // - CMF requires HLCV data and sliding window calculations
        // - First-party implementation is optimized with circular buffers
        // - Use QuantConnect if it's already loaded (for consistency with other indicators)
        // - Default to first-party for its efficiency and direct control

        // Check if QuantConnect assembly is already loaded (to avoid loading it unnecessarily)
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);

        if (quantConnectLoaded)
        {
            // Use QuantConnect for consistency when it's already loaded
            return CreateQuantConnectImplementation(parameters);
        }
        else
        {
            // Use first-party implementation for efficiency
            return new ChaikinMoneyFlow_FP<TInput, TOutput>(parameters);
        }
    }

    /// <summary>
    /// Convenience method to create a CMF indicator for TimedBarStruct input with double output.
    /// </summary>
    /// <param name="period">The period for CMF calculation (default: 21)</param>
    /// <returns>A CMF indicator instance for TimedBarStruct input and double output</returns>
    public static IChaikinMoneyFlow<TimedBarStruct, double> CreateBarDouble(int period = 21)
    {
        return Create<TimedBarStruct, double>(period);
    }

    /// <summary>
    /// Convenience method to create a CMF indicator for TimedBar input with double output.
    /// </summary>
    /// <param name="period">The period for CMF calculation (default: 21)</param>
    /// <returns>A CMF indicator instance for TimedBar input and double output</returns>
    public static IChaikinMoneyFlow<TimedBar, double> CreateTimedBarDouble(int period = 21)
    {
        return Create<TimedBar, double>(period);
    }

    /// <summary>
    /// Convenience method to create a CMF indicator for TimedBarStruct input with decimal output.
    /// </summary>
    /// <param name="period">The period for CMF calculation (default: 21)</param>
    /// <returns>A CMF indicator instance for TimedBarStruct input and decimal output</returns>
    public static IChaikinMoneyFlow<TimedBarStruct, decimal> CreateBarDecimal(int period = 21)
    {
        return Create<TimedBarStruct, decimal>(period);
    }

    /// <summary>
    /// Convenience method to create a CMF indicator for TimedBar input with decimal output.
    /// </summary>
    /// <param name="period">The period for CMF calculation (default: 21)</param>
    /// <returns>A CMF indicator instance for TimedBar input and decimal output</returns>
    public static IChaikinMoneyFlow<TimedBar, decimal> CreateTimedBarDecimal(int period = 21)
    {
        return Create<TimedBar, decimal>(period);
    }
}