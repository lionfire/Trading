namespace LionFire.Trading.Automation;

public class PSimulatedAccount2<TPrecision>
    : IPSimulatedAccount2<TPrecision>
        where TPrecision : struct, INumber<TPrecision>
{
    public TPrecision StartingBalance { get; set; }

    public string? BalanceCurrency { get; set; }
    public TPrecision AbortOnBalanceDrawdownPerunum { get; set; }
   }
