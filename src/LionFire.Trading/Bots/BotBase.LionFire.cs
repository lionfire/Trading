using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{



    public partial class BotBase : MarketParticipant, IBot
    {
        public IAccount Account { get; set; }
        public MarketData MarketData { get; set; }

        public bool IsBacktesting { get; set; }

        public Server Server { get; set; }

        public Symbol Symbol { get; set; }

        protected virtual double GetFitness(GetFitnessArgs args) { return 0.0; }


        public List<Position> Positions { get { return null; } }

        public TradeResult ClosePosition(Position position)
        {
            throw new NotImplementedException();
        }

        public TradeResult ModifyPosition(Position position, double? StopLoss, double? TakeProfit)
        {
            throw new NotImplementedException();
        }

        public TradeResult ExecuteMarketOrder(TradeType tradeType, Symbol symbol, long volumeInUnits, string label, double? stopLossInPips, double? takeProfitInPips = null, double? marketRangePips = null, string comment = null)
        {
            return Account?.ExecuteMarketOrder(tradeType, symbol, volumeInUnits, label, stopLossInPips, takeProfitInPips, marketRangePips, comment);
        }
    }
}
