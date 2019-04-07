namespace LionFire.Trading.Portfolios
{
    public enum PortfolioEquityCurveMode
    {
        Unspecified = 0,

        /// <summary>
        /// Use equity HLC bars from backtest (Not available yet)
        /// </summary>
        Precise = 1,

        /// <summary>
        /// Take the entry and close times, and interpolate equity from the net profit.  This means this does not provide a true equity curve, which may be much less volatile than reality.
        /// </summary>
        InterpolateEquityFromBalance = 2,

        /// <summary>
        /// No equity curve.  Whenever a trade closes, apply that to the portfolio balance.  Makes balance curve look like a step function since changes are applied instantly.
        /// </summary>
        BalanceOnly = 3,
    }
}
