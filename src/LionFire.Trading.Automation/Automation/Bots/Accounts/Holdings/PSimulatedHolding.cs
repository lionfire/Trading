namespace LionFire.Trading.Automation;

public class PSimulatedHolding<TPrecision>
    : IPSimulatedHolding<TPrecision>
        where TPrecision : struct, INumber<TPrecision>
{
    #region (static)

    public static PSimulatedHolding<TPrecision> Default { get; }
    public static PSimulatedHolding<TPrecision> DefaultForBacktesting { get; }

    static PSimulatedHolding()
    {
        Default = new PSimulatedHolding<TPrecision>
        {
            Symbol = ExchangeSymbol.GenericUSD.Symbol!,
            StartingBalance = TPrecision.Zero,
        };

        DefaultForBacktesting = new PSimulatedHolding<TPrecision>
        {
            Symbol = ExchangeSymbol.GenericUSD.Symbol!,
            StartingBalance = TPrecision.CreateChecked(10_000.0),
        };
    }

    #endregion


    public required string Symbol { get; init; } // FUTURE? MOVE to base class PHolding, if needed
    public TPrecision StartingBalance { get; set; }

    public PAssetProtection<TPrecision>? AssetProtection { get; set; }

}
