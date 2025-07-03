namespace LionFire.Trading;

public interface IHolding<TPrecision> : IKeyed<string>
    where TPrecision : struct, INumber<TPrecision>
{

    string Symbol { get; }

    // TODO: Move SimulatedAccount logic into here, and make Balance readonly
    TPrecision Balance { get; set; }

}

public interface ISimHolding<TPrecision> : IHolding<TPrecision>
        where TPrecision : struct, INumber<TPrecision>
{

    #region Stats

    TPrecision BalanceReturnOnInvestment { get; }
    double AnnualizedBalanceRoi { get; }
    double AnnualizedBalanceRoiVsDrawdownPercent { get; }
    double AnnualizedEquityRoiVsDrawdownPercent { get; }

    TPrecision MaxEquityDrawdownPerunum { get; }
    TPrecision CurrentEquityDrawdown { get; }
    TPrecision MaxEquityDrawdown { get; }

    TPrecision MaxBalanceDrawdownPerunum { get; }
    TPrecision CurrentBalanceDrawdown { get; }
    TPrecision MaxBalanceDrawdown { get; }

    #endregion
}
