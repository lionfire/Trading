using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation;

public sealed class SimHolding<TPrecision> : Holding<TPrecision>, ISimHolding<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    #region Parent

    public SimAccount<TPrecision> Account { get; }
    public SimContext<TPrecision> SimContext => Account.SimContext;

    #endregion

    #region Parameters

    new IPSimulatedHolding<TPrecision> PHolding => (IPSimulatedHolding<TPrecision>)base.PHolding;

    #endregion

    #region Lifecycle

    public SimHolding(SimAccount<TPrecision> account, IPSimulatedHolding<TPrecision> pHolding) : base(pHolding)
    {
        InitialBalance = pHolding.StartingBalance;
        Balance = pHolding.StartingBalance;
        Equity = Balance;
        HighestBalance = Balance;
        HighestEquity = Equity;
        Account = account;
    }

    #endregion

    #region State

    #region Equity

    public TPrecision Equity
    {
        get => equity;
        set
        {
            equity = value;

            // OPTIMIZE: defer equity calculations until a 2nd pass once ending balance and balance drawdown is acceptable
            if (equity > HighestEquity) { HighestEquity = equity; }
            else
            {
                var drawdown = CurrentEquityDrawdown;
                if (drawdown > MaxEquityDrawdown) { MaxEquityDrawdown = drawdown; }
                var perunum = drawdown / HighestEquity;
                if (perunum > MaxEquityDrawdownPerunum) { MaxEquityDrawdownPerunum = perunum; }
            }
        }
    }
    private TPrecision equity;

    #endregion

    #region Balance

    public override TPrecision Balance
    {
        get => balance;
        set
        {
            balance = value;
            if (balance > HighestBalance) { HighestBalance = balance; }
            else
            {
                var drawdown = CurrentBalanceDrawdown;
                if (drawdown > MaxBalanceDrawdown) { MaxBalanceDrawdown = drawdown; }
                var perunum = drawdown / HighestBalance;
                if (perunum > MaxBalanceDrawdownPerunum) { MaxBalanceDrawdownPerunum = perunum; }
                if (PHolding.AssetProtection?.AbortOnBalanceDrawdownPerunum != default && perunum > PHolding.AssetProtection?.AbortOnBalanceDrawdownPerunum) { Account.Abort(BotAbortReason.BalanceDrawdown); }
            }
        }
    }

    #endregion

    #region Stats

    #region Stats: Equity

    public TPrecision InitialEquity { get; set; }
    public TPrecision HighestEquity { get; set; }

    #region Derived

    public TPrecision EquityReturnOnInvestment => (Equity - InitialBalance) / InitialBalance;

    public double AnnualizedEquityRoi => Convert.ToDouble(EquityReturnOnInvestment) * ((SimContext.SimulatedCurrentDate - SimContext.MultiSimContext.Parameters.Start).TotalDays / 365);
    public double AnnualizedEquityRoiVsDrawdownPercent => AnnualizedEquityRoi / Convert.ToDouble(MaxEquityDrawdownPerunum);

    #endregion

    #endregion

    #region Stats: Balance 

    public TPrecision InitialBalance { get; set; }
    public TPrecision HighestBalance { get; set; }
    public TPrecision MaxBalanceDrawdown { get; set; }
    public TPrecision MaxBalanceDrawdownPerunum { get; set; }
    public TPrecision MaxEquityDrawdown { get; set; }
    public TPrecision MaxEquityDrawdownPerunum { get; set; }

    #region Derived

    public TPrecision CurrentBalanceDrawdown => HighestBalance - Balance;
    public TPrecision CurrentEquityDrawdown => Equity - HighestEquity;

    public TPrecision BalanceReturnOnInvestment => (Balance - InitialBalance) / InitialBalance;
    public double AnnualizedBalanceRoi => Convert.ToDouble(BalanceReturnOnInvestment) * ((SimContext.SimulatedCurrentDate - Start).TotalDays / 365);
    /// <summary>
    /// Annualized ROI divided by max drawdown (as perunum). Uses a floor of 0.1% drawdown to avoid infinity.
    /// </summary>
    public double AnnualizedBalanceRoiVsDrawdownPercent
    {
        get
        {
            var drawdown = Convert.ToDouble(MaxBalanceDrawdownPerunum);
            // Use a minimum drawdown floor of 0.001 (0.1%) to avoid infinity
            if (drawdown < 0.001) drawdown = 0.001;
            return AnnualizedBalanceRoi / drawdown;
        }
    }


    #endregion

    #endregion

    #endregion

    #endregion

    #region Convenience

    private DateTimeOffset Start => SimContext.MultiSimContext.Parameters.Start;

    #endregion
}
