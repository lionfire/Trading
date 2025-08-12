using LionFire.Structures;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;
using System.Reflection;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Parabolic SAR (Stop and Reverse) indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class ParabolicSAR
{
    /// <summary>
    /// Creates a Parabolic SAR indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The Parabolic SAR parameters</param>
    /// <returns>A Parabolic SAR indicator instance</returns>
    public static IParabolicSAR<HLC<TPrice>, TOutput> Create<TPrice, TOutput>(PParabolicSAR<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.PreferredImplementation switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new ParabolicSAR_FP<TPrice, TOutput>(parameters),
            ImplementationHint.Optimized => new ParabolicSAR_FP<TPrice, TOutput>(parameters), // FP implementation is already optimized
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a Parabolic SAR indicator with default parameters.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="accelerationFactor">The initial acceleration factor (default: 0.02)</param>
    /// <param name="maxAccelerationFactor">The maximum acceleration factor (default: 0.20)</param>
    /// <returns>A Parabolic SAR indicator instance</returns>
    public static IParabolicSAR<HLC<TPrice>, TOutput> Create<TPrice, TOutput>(
        double accelerationFactor = 0.02, 
        double maxAccelerationFactor = 0.20)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PParabolicSAR<TPrice, TOutput> 
        { 
            AccelerationFactor = TOutput.CreateChecked(accelerationFactor),
            MaxAccelerationFactor = TOutput.CreateChecked(maxAccelerationFactor)
        };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IParabolicSAR<HLC<TPrice>, TOutput> CreateQuantConnectImplementation<TPrice, TOutput>(PParabolicSAR<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        try
        {
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var psarqcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.ParabolicSAR_QC`2");
            
            if (psarqcType != null)
            {
                // Make the generic type
                var genericType = psarqcType.MakeGenericType(typeof(TPrice), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IParabolicSAR<HLC<TPrice>, TOutput> psar)
                {
                    return psar;
                }
            }
        }
        catch
        {
            // Fall back to first-party implementation if QuantConnect is not available
        }
        
        // Fallback to first-party implementation
        return new ParabolicSAR_FP<TPrice, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IParabolicSAR<HLC<TPrice>, TOutput> SelectBestImplementation<TPrice, TOutput>(PParabolicSAR<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        // Decision logic for selecting implementation:
        // - The first-party implementation has complete Parabolic SAR algorithm with proper trend tracking
        // - QuantConnect implementation is stable but may have limitations in trend direction exposure
        // - Default to first-party for its complete feature set and algorithm accuracy

        // Check if QuantConnect assembly is already loaded (to avoid loading it unnecessarily)
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);

        if (quantConnectLoaded)
        {
            // Use QuantConnect if it's already loaded for consistency with other indicators
            return CreateQuantConnectImplementation(parameters);
        }
        else
        {
            // Use first-party implementation for complete algorithm and better control
            return new ParabolicSAR_FP<TPrice, TOutput>(parameters);
        }
    }

    /// <summary>
    /// Convenience method to create a Parabolic SAR indicator for double values.
    /// </summary>
    /// <param name="accelerationFactor">The initial acceleration factor</param>
    /// <param name="maxAccelerationFactor">The maximum acceleration factor</param>
    /// <returns>A Parabolic SAR indicator instance for double values</returns>
    public static IParabolicSAR<HLC<double>, double> CreateDouble(
        double accelerationFactor = 0.02, 
        double maxAccelerationFactor = 0.20)
    {
        return Create<double, double>(accelerationFactor, maxAccelerationFactor);
    }

    /// <summary>
    /// Convenience method to create a Parabolic SAR indicator for decimal values.
    /// </summary>
    /// <param name="accelerationFactor">The initial acceleration factor</param>
    /// <param name="maxAccelerationFactor">The maximum acceleration factor</param>
    /// <returns>A Parabolic SAR indicator instance for decimal values</returns>
    public static IParabolicSAR<HLC<decimal>, decimal> CreateDecimal(
        decimal accelerationFactor = 0.02m, 
        decimal maxAccelerationFactor = 0.20m)
    {
        return Create<decimal, decimal>(Convert.ToDouble(accelerationFactor), Convert.ToDouble(maxAccelerationFactor));
    }

    /// <summary>
    /// Convenience method to create a Parabolic SAR indicator for float values.
    /// </summary>
    /// <param name="accelerationFactor">The initial acceleration factor</param>
    /// <param name="maxAccelerationFactor">The maximum acceleration factor</param>
    /// <returns>A Parabolic SAR indicator instance for float values</returns>
    public static IParabolicSAR<HLC<float>, float> CreateFloat(
        float accelerationFactor = 0.02f, 
        float maxAccelerationFactor = 0.20f)
    {
        return Create<float, float>(accelerationFactor, maxAccelerationFactor);
    }
}