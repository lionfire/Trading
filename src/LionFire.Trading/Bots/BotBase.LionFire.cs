using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{



    public partial class BotBase<TConfig> : MarketParticipant, IBot
    {
        public IAccount Account { get { return Market.Account; } }
        public MarketData MarketData { get; set; }

        public bool IsBacktesting { get; set; }

        public Server Server { get { return Market.Server; } }

        public Symbol Symbol { get; set; }

#if cAlgo
        protected
#else
        public
#endif
        virtual double GetFitness(GetFitnessArgs args) { return 0.0; }


        public IPositions Positions { get { return Account.Positions; } }

        public TradeResult ClosePosition(Position position)
        {
            return Account?.ClosePosition(position);
        }

        public TradeResult ModifyPosition(Position position, double? StopLoss, double? TakeProfit)
        {
            return Account?.ModifyPosition(position, StopLoss, TakeProfit);
        }

        public TradeResult ExecuteMarketOrder(TradeType tradeType, Symbol symbol, long volumeInUnits, string label, double? stopLossInPips, double? takeProfitInPips = null, double? marketRangePips = null, string comment = null)
        {
            if (!CanOpen)
            {
                logger.LogWarning("OpenPosition called but CanOpen is false.");
                return TradeResult.LimitedByConfig;
            }
            if (tradeType == TradeType.Buy && !CanOpenLong)
            {
                logger.LogWarning("OpenPosition called but CanOpenLong is false.");
                return TradeResult.LimitedByConfig;
            }
            else if (tradeType == TradeType.Buy && !CanOpenShort)
            {
                logger.LogWarning("OpenPosition called but CanOpenShort is false.");
                return TradeResult.LimitedByConfig;
            }

            return Account?.ExecuteMarketOrder(tradeType, symbol, volumeInUnits, label, stopLossInPips, takeProfitInPips, marketRangePips, comment);
        }


    }
}
