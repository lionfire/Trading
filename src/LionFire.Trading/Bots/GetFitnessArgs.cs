using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{
    public interface GetFitnessArgs
    {
        double AverageTrade { get; }
        double Equity { get; }
        History History { get; }
        double LosingTrades { get; }
        double MaxBalanceDrawdown { get; }
        double MaxBalanceDrawdownPercentages { get; }
        double MaxEquityDrawdown { get; }
        double MaxEquityDrawdownPercentages { get; }
        double NetProfit { get; }
        //PendingOrders PendingOrders { get; } TODO
        //Positions Positions { get; } TODO
        double ProfitFactor { get; }
        double SharpeRatio { get; }
        double SortinoRatio { get; }
        double TotalTrades { get; }
        double WinningTrades { get; }
    }

    
}
