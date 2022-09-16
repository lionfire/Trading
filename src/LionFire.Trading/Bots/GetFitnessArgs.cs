using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface GetFitnessArgs
    {
        double AverageTrade { get; }
        double AverageTradePerVolume { get; }
        double Equity { get; }
        History History { get; }
        int LosingTrades { get; }
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
        int TotalTrades { get; }
        int WinningTrades { get; }
    }

    public class _GetFitnessArgs : GetFitnessArgs
    {
        public double AverageTrade {
            get;set;
        }
        public double AverageTradePerVolume { get; set; }

        public double Equity {
            get; set;
        }

        public History History {
            get; set;
        }

        public int LosingTrades {
            get; set;
        }

        public double MaxBalanceDrawdown {
            get; set;
        }

        public double MaxBalanceDrawdownPercentages {
            get; set;
        }

        public double MaxEquityDrawdown {
            get; set;
        }

        public double MaxEquityDrawdownPercentages {
            get; set;
        }

        public double NetProfit {
            get; set;
        }

        public double ProfitFactor {
            get; set;
        }

        public double SharpeRatio {
            get; set;
        }

        public double SortinoRatio {
            get; set;
        }

        public int TotalTrades {
            get; set;
        }

        public int WinningTrades {
            get; set;
        }
        
    }

}
