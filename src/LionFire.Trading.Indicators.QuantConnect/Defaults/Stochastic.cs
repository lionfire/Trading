using LionFire.Trading.Indicators.Parameters;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// Default Stochastic Oscillator implementation. Points to QuantConnect implementation for stability and compatibility.
/// </summary>
/// <remarks>
/// This alias allows easy switching between implementations without changing client code.
/// Use Stochastic_QC directly if you need the QuantConnect-specific implementation,
/// or Stochastic_FP from LionFire.Trading.Indicators.Native for the first-party implementation.
/// </remarks>
public class Stochastic<TPrice, TOutput> : Stochastic_QC<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, System.Numerics.INumber<TOutput>
{
    public Stochastic(PStochastic<TPrice, TOutput> parameters) : base(parameters) { }
    
    public static new Stochastic<TPrice, TOutput> Create(PStochastic<TPrice, TOutput> p)
        => new Stochastic<TPrice, TOutput>(p);
}