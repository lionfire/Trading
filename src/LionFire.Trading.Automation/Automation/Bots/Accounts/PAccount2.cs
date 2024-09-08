//#define BacktestAccountSlottedParameters // FUTURE Maybe, though I think we just typically need 1 hardcoded slot for the bars
namespace LionFire.Trading.Automation;

public class PAccount2<TPrecision>
    : IPAccount2<TPrecision>
        where TPrecision : struct, INumber<TPrecision>
{
    public TPrecision StartingBalance { get; set; }

    public string? BalanceCurrency { get; set; }
}
