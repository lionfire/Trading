
using System;

namespace LionFire.Trading.Backtesting;

public static class BacktestStats
{

    #region DAD - Duration Adjusted AD

    public const double AboveOneExpPower = 1.5;
    public const double BelowOnePower = 4;
    
    public static double DurationAdjustedAroiVsDrawdown(BacktestResult backtest)
        => DurationAdjustedAroiVsDrawdown(backtest.Duration.TotalDays, backtest.AD);

    public static double DurationAdjustedAroiVsDrawdown(double ad, double days)
    {
        var years = days / 365.0;
        if (years >= 1) return ad + ad * Math.Log(years, Math.Pow(Math.E, AboveOneExpPower));
        else return ad * Math.Pow(BelowOnePower, Math.Log(years, Math.E));
    }

    #endregion

    #region NAD

    public const double PenaltyAmount = 0.8;
    public const double BaselineTradeCount = 40;

    /// <summary>
    /// NAD - Number of Trades adjusted Aroi/Drawdown: Penalize if below a certain number of trades (such as 30-40)
    /// </summary>
    /// <param name="ad"></param>
    /// <param name="tradeCount"></param>
    /// <returns></returns>
    public static double Nad(double ad, int tradeCount)
    {
        // AD * Power(1-MAX(0,(BaselineTradeCount-tradeCount)/BaselineTradeCount), PenaltyAmount)
        if (tradeCount >= BaselineTradeCount) { return ad; }

        return ad * Math.Pow(1 - ((BaselineTradeCount - tradeCount) / BaselineTradeCount), PenaltyAmount);
    }

    #endregion

    #region PAD - Profit average Adjusted Drawdown

    public const double PadPenaltyAmount = 0.8;
    public const double PadBaselineAmount = 0.01;

    /// <summary>
    /// ProfitAdjustedAroiVsDrawdown: Punish if profit margins are low
    /// </summary>
    /// <param name="ad"></param>
    /// <param name="averageTradePerVolume"></param>
    /// <returns></returns>
    public static double Pad(BacktestResult r)
    {
        // TODO: Is there an easy/realiable way to get position size?
        // AD * Power(1-MAX(0,(PadBaselineTradeCount-tradeCount)/PadBaselineTradeCount), PadBaselineAmount)
        if (r.AverageTradePerVolume >= PadBaselineAmount) { return r.AD; }

        return r.AD * Math.Pow(1 - ((PadBaselineAmount - r.AverageTradePerVolume) / PadBaselineAmount), PadPenaltyAmount);
    }

    #endregion

    #region AAD

    public static double AdjustedAroiVsDrawdown(BacktestResult r)
    {
        var dad = DurationAdjustedAroiVsDrawdown(r.AD, r.Days);
        var nad = Nad(r.AD, r.TotalTrades);
        //var pad = Pad(r.AD, r.AverageTradePerVolume); // TODO - add this in if useful

        return (dad + nad) / 2;
    }

    #endregion
}
