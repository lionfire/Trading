using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.DataFlow.Indicators;
using LionFire.Trading;
using System.Numerics;
using System.Reflection;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Accumulation/Distribution Line indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class AccumulationDistributionLine
{
    /// <summary>
    /// Creates an Accumulation/Distribution Line indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TInput">The input data type (should have High, Low, Close and Volume properties)</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The A/D Line parameters</param>
    /// <returns>An A/D Line indicator instance</returns>
    public static IAccumulationDistributionLine<TInput, TOutput> Create<TInput, TOutput>(PAccumulationDistributionLine<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.PreferredImplementation switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new AccumulationDistributionLine_FP<TInput, TOutput>(parameters),
            ImplementationHint.Optimized => new AccumulationDistributionLine_FP<TInput, TOutput>(parameters), // FP is already optimized
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates an Accumulation/Distribution Line indicator with default parameters.
    /// </summary>
    /// <typeparam name="TInput">The input data type (should have High, Low, Close and Volume properties)</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <returns>An A/D Line indicator instance</returns>
    public static IAccumulationDistributionLine<TInput, TOutput> Create<TInput, TOutput>()
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PAccumulationDistributionLine<TInput, TOutput>();
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IAccumulationDistributionLine<TInput, TOutput> CreateQuantConnectImplementation<TInput, TOutput>(PAccumulationDistributionLine<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        try
        {
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var adQcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.AccumulationDistributionLine_QC`2");
            
            if (adQcType != null)
            {
                // Make the generic type
                var genericType = adQcType.MakeGenericType(typeof(TInput), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IAccumulationDistributionLine<TInput, TOutput> ad)
                {
                    return ad;
                }
            }
        }
        catch
        {
            // Fall back to first-party implementation if QuantConnect is not available
        }
        
        // Fallback to first-party implementation
        return new AccumulationDistributionLine_FP<TInput, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IAccumulationDistributionLine<TInput, TOutput> SelectBestImplementation<TInput, TOutput>(PAccumulationDistributionLine<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        // Decision logic for selecting implementation:
        // - A/D Line is relatively simple, so first-party implementation is efficient
        // - Use QuantConnect if it's already loaded (for consistency with other indicators)
        // - Default to first-party for its simplicity and direct control

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
            return new AccumulationDistributionLine_FP<TInput, TOutput>(parameters);
        }
    }

    /// <summary>
    /// Convenience method to create an A/D Line indicator for TimedBarStruct input with double output.
    /// </summary>
    /// <returns>An A/D Line indicator instance for TimedBarStruct input and double output</returns>
    public static IAccumulationDistributionLine<TimedBarStruct, double> CreateBarDouble()
    {
        return Create<TimedBarStruct, double>();
    }

    /// <summary>
    /// Convenience method to create an A/D Line indicator for TimedBar input with double output.
    /// </summary>
    /// <returns>An A/D Line indicator instance for TimedBar input and double output</returns>
    public static IAccumulationDistributionLine<TimedBar, double> CreateTimedBarDouble()
    {
        return Create<TimedBar, double>();
    }

    /// <summary>
    /// Convenience method to create an A/D Line indicator for TimedBarStruct input with decimal output.
    /// </summary>
    /// <returns>An A/D Line indicator instance for TimedBarStruct input and decimal output</returns>
    public static IAccumulationDistributionLine<TimedBarStruct, decimal> CreateBarDecimal()
    {
        return Create<TimedBarStruct, decimal>();
    }

    /// <summary>
    /// Convenience method to create an A/D Line indicator for TimedBar input with decimal output.
    /// </summary>
    /// <returns>An A/D Line indicator instance for TimedBar input and decimal output</returns>
    public static IAccumulationDistributionLine<TimedBar, decimal> CreateTimedBarDecimal()
    {
        return Create<TimedBar, decimal>();
    }
}