using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Accounts
{

    public class BacktestAccount : MarketParticipant, IAccount
    {
        public string Currency {
            get; set;
        } = "USD";

        public double Equity {
            get; set;
        }
        public double Balance {
            get; set;
        }

        public List<Position> Positions { get; private set; } = new List<Position>();

        public double StopOutLevel { get { return BacktestSettings.AccountSettings.StopOutLevel; } }

        public bool IsDemo {
            get { return true; }
        }

        public BacktestSettings BacktestSettings { get; set; }

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


        }
    }
}
