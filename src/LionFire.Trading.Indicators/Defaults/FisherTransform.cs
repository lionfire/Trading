using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.QuantConnect_;
using System.Numerics;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Fisher Transform factory that selects the best implementation
/// </summary>
public static class FisherTransform
{
    /// <summary>
    /// Create Fisher Transform with automatic implementation selection
    /// </summary>
    public static IFisherTransform<TInput, TOutput> Create<TInput, TOutput>(
        PFisherTransform<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.PreferredImplementation switch
        {
            ImplementationHint.FirstParty => FisherTransform_FP<TInput, TOutput>.Create(parameters),
            ImplementationHint.QuantConnect => FisherTransform_QC<TInput, TOutput>.Create(parameters),
            ImplementationHint.Optimized => FisherTransform_FP<TInput, TOutput>.Create(parameters), // Use FP as optimized for now
            _ => FisherTransform_FP<TInput, TOutput>.Create(parameters) // Default to FP implementation
        };
    }

    /// <summary>
    /// Create Fisher Transform with default parameters
    /// </summary>
    public static IFisherTransform<TInput, TOutput> Create<TInput, TOutput>(int period = 10)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PFisherTransform<TInput, TOutput>
        {
            Period = period
        };
        return Create(parameters);
    }
}

/// <summary>
/// Generic Fisher Transform indicator for common numeric types
/// </summary>
public static class FisherTransform<TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// Create Fisher Transform with specified parameters
    /// </summary>
    public static IFisherTransform<TInput, TOutput> Create(PFisherTransform<TInput, TOutput> parameters)
        => FisherTransform.Create(parameters);

    /// <summary>
    /// Create Fisher Transform with default parameters
    /// </summary>
    public static IFisherTransform<TInput, TOutput> Create(int period = 10)
        => FisherTransform.Create<TInput, TOutput>(period);
}

/// <summary>
/// Commonly used Fisher Transform with decimal precision
/// </summary>
public static class FisherTransformDecimal
{
    /// <summary>
    /// Create Fisher Transform with decimal input/output
    /// </summary>
    public static IFisherTransform<decimal, decimal> Create(int period = 10)
        => FisherTransform.Create<decimal, decimal>(period);

    /// <summary>
    /// Create Fisher Transform with custom parameters
    /// </summary>
    public static IFisherTransform<decimal, decimal> Create(PFisherTransform<decimal, decimal> parameters)
        => FisherTransform.Create(parameters);
}

/// <summary>
/// Commonly used Fisher Transform with double precision  
/// </summary>
public static class FisherTransformDouble
{
    /// <summary>
    /// Create Fisher Transform with double input/output
    /// </summary>
    public static IFisherTransform<double, double> Create(int period = 10)
        => FisherTransform.Create<double, double>(period);

    /// <summary>
    /// Create Fisher Transform with custom parameters
    /// </summary>
    public static IFisherTransform<double, double> Create(PFisherTransform<double, double> parameters)
        => FisherTransform.Create(parameters);
}