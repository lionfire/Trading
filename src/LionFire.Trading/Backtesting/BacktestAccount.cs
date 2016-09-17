using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Backtesting
{

    public class BacktestAccount : MarketParticipant, IAccount
    {
        public string Currency {
            get; set;
        }

        public double Equity {
            get; set;
        }
        public double Balance {
            get; set;
        }

        public double MarginUsed { get; set; }

        #region Derived

        public double MarginLevel { get { return Equity / MarginUsed; } }
        public double MarginLevelPercent { get { return 100.0 * Equity / MarginUsed; } }

        public double FreeMargin { get { return Equity - MarginUsed; } }
        
        #endregion
        


        IPositions IAccount.Positions { get { return this.Positions; } }
        public Positions Positions { get; private set; } = new Positions();

        public double StopOutLevel { get { return BacktestSettings.AccountSettings.StopOutLevel; } }

        public bool IsDemo {
            get { return true; }
        }

        public BacktestConfig BacktestSettings { get; set; }

        protected override void OnStarting()
        {
            base.OnStarting();

            Equity = Balance = BacktestSettings.StartingBalance;
        }

        public int positionCounter = 1;

        public TradeResult ExecuteMarketOrder(TradeType tradeType, Symbol symbol, long volume, string label = null, double? stopLossPips = default(double?), double? takeProfitPips = default(double?), double? marketRangePips = default(double?), string comment = null)
        {
            var slippage = 0;
            var entryPrice = tradeType == TradeType.Buy ? symbol.Bid + slippage : symbol.Ask - slippage;

            var p = new Position()
            {
                Comment = comment,
                Id = positionCounter++,
                EntryTime = Market.SimulationTime,
                EntryPrice = entryPrice,
                Commissions = BacktestSettings.AccountSettings.DefaultSymbolSettings.CommissionPerMillion / volume,
                Label = "Backtest",
                SymbolCode = symbol.Code,
                TradeType = tradeType,
                Volume = volume,

            };
            Positions.Add(p);

            return new TradeResult
            {
                Error = ErrorCode.TechnicalError
            };

        }

        public override void OnBar(string symbolCode, TimeFrame timeFrame, TimedBar bar)
        {
            base.OnBar(symbolCode, timeFrame, bar);

            var equity = Balance;

            foreach (var position in Positions)
            {
                equity += position.NetProfit;
            }

            

        }

        public override void OnTick(SymbolBar bar)
        {
            base.OnTick(bar);
        }

    }
}
