using LionFire.Trading.Indicators.Parameters;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Bollinger Bands implementation. Points to First-Party implementation for efficiency.
/// </summary>
/// <remarks>
/// This alias allows easy switching between implementations without changing client code.
/// Use BollingerBandsQC directly if you need the QuantConnect-specific implementation,
/// or BollingerBandsFP from LionFire.Trading.Indicators.Native for the first-party implementation.
/// 
/// The default points to the First-Party implementation as it's optimized for streaming data
/// and doesn't have external dependencies.
/// </remarks>
public class BollingerBands<TInput, TOutput> : Native.BollingerBandsFP<TInput, TOutput>
    where TInput : struct
    where TOutput : struct, System.Numerics.INumber<TOutput>
{
    public BollingerBands(PBollingerBands<TInput, TOutput> parameters) : base(parameters) { }
    
    public static new BollingerBands<TInput, TOutput> Create(PBollingerBands<TInput, TOutput> p)
        => new BollingerBands<TInput, TOutput>(p);
}