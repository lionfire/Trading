namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// Default Money Flow Index (MFI) implementation. Points to QuantConnect implementation for stability and compatibility.
/// </summary>
/// <remarks>
/// This alias allows easy switching between implementations without changing client code.
/// Use MFI_QC directly if you need the QuantConnect-specific implementation,
/// or MFI_FP from LionFire.Trading.Indicators.Native for the first-party implementation.
/// </remarks>
public class MFI<TInput, TOutput> : MFI_QC<TInput, TOutput>
{
    public MFI(PMFI<TInput, TOutput> parameters) : base(parameters) { }
    
    public static new MFI<TInput, TOutput> Create(PMFI<TInput, TOutput> p)
        => new MFI<TInput, TOutput>(p);
}