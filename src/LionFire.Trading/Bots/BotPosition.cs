#if cAlgo
using cAlgo.API;
using cAlgo.API.Internals;
#endif
using System.Diagnostics;

using LionFire.Trading.Bots;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading
{

    public class BotPosition : IOnBar
    {

        #region Ontology

        public IBot Bot
        {
            get { return bot; }
        }
        IBot bot;
        public ISingleChartBot SingleChartBot { get { return bot as ISingleChartBot; } }
        public ISingleChartBotEx BotEx
        {
            get { return bot as ISingleChartBotEx; }
        }

        public Position Position
        {
            get { return position; }
        }
        Position position;

        public Symbol Symbol
        {
            get { return symbol; }
        }
        Symbol symbol;

        #endregion

        #region State


        public double? MinStopLoss
        {
            get { return minStopLoss; }
            set
            {
                if (minStopLoss == value)
                    return;
                minStopLoss = value;
                //    l.LogTrace("[MIN SL] " + value);
            }
        }
        private double? minStopLoss;
        public double? MaxStopLoss;
        public double? MinTakeProfit;
        public double? MaxTakeProfit;

        public int DefaultBarsSinceOpen = 0;
        public TimeFrame DefaultTimeFrame
        {
            get; set;
        }

        public double NewStopLoss = double.NaN;


        public double ClosePoints { get; set; }
        public double BarsSinceLastClosePointsIncrease { get; set; }

        #endregion

        #region State (Derived)

        public int BarsSinceOpen(TimeFrame timeFrame)
        {
            if (timeFrame == DefaultTimeFrame)
            {
                return DefaultBarsSinceOpen;
            }
            throw new NotImplementedException("timeFrame != DefaultTimeFrame");
        }

        #endregion


        public BotPosition(Position position, IBot bot, Symbol symbol, TradeType tradeType, double stopLossInPips, double takeProfitInPips, double volumeInUnits)
        {
            this.bot = bot;
            this.position = position;
            this.symbol = symbol;
#if !cAlgo
            if (position == null)
            {
                double sl = symbol.GetStopLossFromPips(tradeType, stopLossInPips);
                double tp = symbol.GetTakeProfitFromPips(tradeType, takeProfitInPips);
                position = new Position()
                {
                    TradeType = tradeType,
                    StopLoss = sl,
                    TakeProfit = tp,
                    Volume = volumeInUnits,
                };
                Debug.WriteLine($"[BOT POSITION] {this}");
            }
#endif
            this.DefaultTimeFrame = (bot as ISingleChartBot)?.TimeFrame;
            this.onBars = new List<IOnBar>();
            this.MinStopLoss = position.StopLoss;
            this.MinTakeProfit = position.TakeProfit;
        }

        /*public void OnTick(object sender)
        {
            Robot bot = (Robot)sender;
            TimeFrame timeFrame = bot.TimeFrame;
            
            CloseIfStopLossTriggered();
            CloseIfTakeProfitTriggered()
        }*/

        public void CloseIfStopLossTriggered()
        {
            //l.LogTrace("[sl close debug] SL: " + StopLoss + " Ask: " + symbol.Ask + " " + TradeMath.IsStopLossTriggered(StopLoss, position, symbol));
            if (TradeMath.IsStopLossTriggered(StopLoss, position, symbol))
            {
                /* if (position.TradeType == TradeType.Sell)
                {
                    l.LogTrace("[SL CLOSE] Ask: " + symbol.Ask + " SL:" + StopLoss);
                }
                else
                {
                    l.LogTrace("[SL CLOSE] Bid: " + symbol.Bid + " SL:" + StopLoss);
                }*/
                bot.ClosePosition(position);
                return;
            }
        }
        public void CloseIfTakeProfitTriggered()
        {
            if (TradeMath.IsTakeProfitTriggered(TakeProfit, position, symbol))
            {
                //l.LogTrace("[TP CLOSE]");
                bot.ClosePosition(position);
                return;
            }
        }

        public double? StopLoss;
        public double? TakeProfit;



        public double? StopLoss_
        {
            get
            {
                var val = TradeMath.Constrain(position.StopLoss, MinStopLoss, MaxStopLoss, position.TradeType);

                if (val.HasValue && double.IsNaN(val.Value))
                {
                    // If Min & Max conflict, take the most aggressive to close the position
                    l.LogTrace("SL: Min " + MinStopLoss + " > Max* " + MaxStopLoss);
                    val = MaxStopLoss;
                }
#if TRACE_SL
                else
                {
                    l.LogTrace("SL=" + val);
                }
#endif
                return val;
            }
        }
        public double? TakeProfit_
        {
            get
            {
                var val = TradeMath.Constrain(position.TakeProfit, MinTakeProfit, MaxTakeProfit, position.TradeType);
                if (val.HasValue && double.IsNaN(val.Value))
                {
                    if (MinTakeProfit.HasValue && !double.IsNaN(MinTakeProfit.Value))
                    {
                        // If Min & Max conflict, take the most aggressive to close the position
                        l.LogTrace("TP: Min* " + MinTakeProfit + " > Max " + MaxTakeProfit);
                        val = MinTakeProfit;
                    }
                    else
                    {
                        if (MaxTakeProfit.HasValue && !double.IsNaN(MaxTakeProfit.Value))
                        {
                            val = MaxTakeProfit.Value;
                        }
                        else
                        {
                            val = null;
                        }
                    }
                }
                return val;
            }
        }

        public List<IOnBar> onBars { get; set; }

        public void OnBar(object sender, TimeFrame timeFrame)
        {
            double lastClosePoints = ClosePoints;

            DefaultBarsSinceOpen++;

            foreach (var hasOnBar in onBars)
            {
                hasOnBar.OnBar(sender, timeFrame);
            }

            TakeProfit = TakeProfit_;
            StopLoss = StopLoss_;

            CloseIfStopLossTriggered();
            CloseIfTakeProfitTriggered();

            var botEx = bot as ISingleChartBotEx;
            if (botEx != null)
            {

                if ((position.TradeType == TradeType.Buy && (StopLoss <= Symbol.Bid && StopLoss - position.StopLoss >= Symbol.TickSize || TakeProfit >= Symbol.Bid && TakeProfit - position.TakeProfit >= Symbol.TickSize)) || (position.TradeType == TradeType.Sell && (StopLoss >= Symbol.Ask && position.StopLoss - StopLoss >= Symbol.TickSize || TakeProfit <= Symbol.Ask && position.TakeProfit - TakeProfit >= Symbol.TickSize)))            /*if (!TakeProfit.AboutEquals(position.TakeProfit, Symbol.PipSize / 10.0) 
             || !StopLoss.AboutEquals(position.StopLoss, Symbol.PipSize / 10.0))*/
                {
                    //l.LogTrace("[MOD] Modifying position: SL:" + StopLoss + " (was " + position.StopLoss + ") TP:" + TakeProfit + " (was: " + position.TakeProfit + ")");
                    bot.ModifyPosition(position, StopLoss, botEx.UseTakeProfit ? TakeProfit : double.NaN);
                }
                else if (StopLoss.HasValue && position.StopLoss.HasValue && StopLoss != position.StopLoss)
                {
                    //                l.LogTrace("[mod sl debug] (Not modifying SL) StopLoss (" + StopLoss.Value + ") != position.StopLoss (" + position.StopLoss.Value + ")  pip: " + Symbol.PipSize + " diff:" + (StopLoss.Value - position.StopLoss.Value) + " abs(diff)-pip: " + (Math.Abs(StopLoss.Value - position.StopLoss.Value) - Convert.ToDouble(Symbol.PipSize)));

                }


                if (lastClosePoints == ClosePoints)
                {
                    if (ClosePoints > 0 && ++BarsSinceLastClosePointsIncrease >= botEx.ClosePointsBackoffFreezeBars)
                    {
                        ClosePoints -= botEx.ClosePointsBackoff;

#if TRACE_ClosePoints
                    l.LogTrace("[closepoints backoff] " + ClosePoints);
#endif
                    }
                }
            }
        }

        #region Misc

        public Microsoft.Extensions.Logging.ILogger Logger { get { return l; } }
        private Microsoft.Extensions.Logging.ILogger l { get { return bot?.Logger; } }

        #endregion
    }
    /*
            if(sl.HasValue && (position.TradeType sl.Value > bot.Symbol.Ask)
            var tp = TradeMath.Constrain(position.TakeProfit, MinTakeProfit, MaxTakeProfit, position.TradeType);
            if(TradeMath.GreaterOrEqual(MinStopLoss, position.StopLoss, position.TradeType))
            {
            }
            */

}
