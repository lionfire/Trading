using LionFire.Trading.Indicators.Parameters;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// Default CCI implementation. Points to QuantConnect implementation for stability and compatibility.
/// </summary>
/// <remarks>
/// This alias allows easy switching between implementations without changing client code.
/// Use CCI_QC directly if you need the QuantConnect-specific implementation,
/// or CCI_FP from LionFire.Trading.Indicators.Native for the first-party implementation.
/// </remarks>
public class CCI<TPrice, TOutput> : CCI_QC<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    public CCI(PCCI<TPrice, TOutput> parameters) : base(parameters) { }
    
    public static new CCI<TPrice, TOutput> Create(PCCI<TPrice, TOutput> p)
        => new CCI<TPrice, TOutput>(p);
}