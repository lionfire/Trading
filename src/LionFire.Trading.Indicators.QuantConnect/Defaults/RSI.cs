namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// Default RSI implementation. Points to QuantConnect implementation for stability and compatibility.
/// </summary>
/// <remarks>
/// This alias allows easy switching between implementations without changing client code.
/// Use RSI_QC directly if you need the QuantConnect-specific implementation,
/// or RSI_FP from LionFire.Trading.Indicators.Native for the first-party implementation.
/// </remarks>
public class RSI<TPrice, TOutput> : RSI_QC<TPrice, TOutput>
{
    public RSI(PRSI<TPrice, TOutput> parameters) : base(parameters) { }
    
    public static new RSI<TPrice, TOutput> Create(PRSI<TPrice, TOutput> p)
        => new RSI<TPrice, TOutput>(p);
}