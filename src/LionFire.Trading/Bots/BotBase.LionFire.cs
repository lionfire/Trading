﻿#if !cAlgo
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LionFire.Execution;
using LionFire.DependencyInjection;
using LionFire.Instantiating;
using LionFire.States;

namespace LionFire.Trading.Bots
{


    public partial class BotBase<TBotType> : BotBase
    {
        //public override bool CanStart
        //{
        //    get
        //    {
        //        return this.Template.TimeFrame == "h1"; // TEMP FIXME - h1 only!
        //    }
        //}
    }

    public partial class BotBase : AccountParticipant, IBot, IStartable, IStoppable
    {

        protected override void OnStopping()
        {
            base.OnStopping();
            OnBotStopping();
        }

        #region Relationships

        public Server Server { get { return base.Account.Server; } }

        #endregion

        #region Configuration

        public bool IsBacktesting { get; set; }

        #endregion

        #region Data

        public MarketData MarketData { get; set; }
        
        [RequiredToEnterState(ExecutionStateEx.Starting)]
        public Symbol Symbol
        {
            get;
            protected set; // Set in OnStarted
            //get
            //{
            //    if (symbol == null && Template.Symbol != null && Market != null)
            //    {
            //        symbol = Market.GetSymbol(Template.Symbol);
            //    }
            //    return symbol;
            //}
        }
        //private Symbol symbol;

        #endregion

        #region Positions

        public IPositionsDouble Positions
        {
            get
            {
                return Account?.Positions;
            }
        }

        #region Position Methods

        public TradeResult ClosePosition(PositionDouble position)
        {
            return Account?.ClosePosition(position);
        }

        public TradeResult ModifyPosition(PositionDouble position, double? StopLoss, double? TakeProfit)
        {
            return Account?.ModifyPosition(position, StopLoss, TakeProfit);
        }

        public TradeResult ExecuteMarketOrder(TradeType tradeType, Symbol symbol, double volumeInUnits, string label, double? stopLossInPips, double? takeProfitInPips = null, double? marketRangePips = null, string comment = null)
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

        #endregion

        #endregion
    }
}
#endif