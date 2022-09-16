#if cAlgo
using cAlgo.API;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Bots.Features
{    

    public class StopLossTrailer : IOnBar
    {
        #region Relationships

        StopLossTrailerConfig config;
        BotPosition position;

        ILogger l => position.Logger;

        #endregion

        #region Construction

        public StopLossTrailer(BotPosition position, StopLossTrailerConfig config)
        {
            this.position = position;
            this.config = config;
        }
        
        #endregion

        #region Convenience Accessors
        
        public RangedNumber Key { get { return config?.Input; } }
        public RangedNumber Value { get { return config?.StopLossLocation; } }
        public Func<double, double> Function { get { return config?.Function; } }
        
        #endregion

        // RENAME Value Trigger
        public bool IncreaseOnly = true;
        

        public double BreakEvenTriggerSpreadMultiple = 5;
        public double Spread => position.Symbol.CurrentSpread();

        public double Cap(bool increaseOnly, double? existingValue, double newValue, TradeType tradeType)
        {

            if (!increaseOnly || !existingValue.HasValue)
            {
                return newValue;
            }

            if (tradeType == TradeType.Buy)
            {
                return Math.Max(existingValue.Value, newValue);
            }
            else
            {
                return Math.Min(existingValue.Value, newValue);
            }
        }

        public double GetSL(double factor)
        {
            //l.LogTrace("GetSL " + factor);
            switch (Value.Unit)
            {
                case Unit.Profit:
                    //l.LogTrace("GetSL - profit: " + position.Position.NetProfit + " factor: " + factor + " Value.Number: " + Value.Number);
                    return (position.Position.TradeType == TradeType.Buy) ? (Math.Max(0.0, position.Position.NetProfit) * factor * Value.Number + position.Position.EntryPrice) : (position.Position.EntryPrice - Math.Max(0.0, position.Position.NetProfit) * factor * Value.Number);
                default:
                    l.LogTrace("Unknown ValueUnit: " + Value.Unit);
                    return double.NaN;

            }
        }

        public double MidPoint {
            // TODO: get this from the position
            // TODO: Alter midpoint depending on ClosePoints and other factors 
            // TODO: Adaptive midpoint based on memory
            // TODO: Current value instead of last?
            get { return position.BotEx.MidChannel; }
        }
        public double HighPoint {
            get { return position.BotEx.HighChannel; }
        }
        public double LowPoint {
            get { return position.BotEx.LowChannel; }
        }

        public void OnBar(object sender, TimeFrame timeFrame)
        {
            Update(timeFrame);
        }
            public void Update(TimeFrame timeFrame)
        {

            switch (Key.Unit)
            {
                case Unit.ClosePoints:
                    var val = DoubleFunctions.Lerp(Value.StartNumber, Value.Number, Key.StartNumber, Key.Number, position.ClosePoints);

                    //var val = (position.ClosePoints * Key.Number) + ((1.0 - position.ClosePoints) * Key.StartNumber);
#if TRACE_ATSL_Channel
                    l.LogTrace("[close] ClosePoints is " + position.ClosePoints + ".  SL is " + val + " " + Value.Unit);
#endif

                    switch (Value.Unit)
                    {
                        case Unit.NearChannel:
                            double sl;
                            if (position.Position.TradeType == TradeType.Buy)
                            {
                                sl = val * (HighPoint) + ((1 - val) * MidPoint);
                            }
                            else
                            {
                                sl = val * (LowPoint) + ((1 - val) * MidPoint);
                            }
                            sl = Cap(IncreaseOnly, position.MinStopLoss, sl, position.Position.TradeType);


#if TRACE_ATSL_Channel
                            if (sl != position.StopLoss)
                            {
                                l.LogTrace("[sp closepoints channel] %:" + val + " SL: " + sl + " H:" + HighPoint + " M: " + MidPoint + " L: " + LowPoint);
                            }
#endif
                            position.StopLoss = sl;
                            break;
                        default:
                            l.LogTrace("Unknown Value.Unit for ClosePoints: " + Value.Unit);
                            break;
                    }
                    break;

                case Unit.Bars:
                    // FIXME  review x/y
                    var spreadAmount = Spread * position.Symbol.PipSize;
                    if (position.Position.TradeType == TradeType.Buy)
                    {

                        if (position.Symbol.Bid < (position.Position.EntryPrice + spreadAmount * BreakEvenTriggerSpreadMultiple))
                        {
                            l.LogTrace("Not moving Long SL until Bid surpasses " + (position.Position.EntryPrice + spreadAmount * BreakEvenTriggerSpreadMultiple));
                            break;
                        }
                    }
                    else
                    {
                        if (position.Symbol.Ask > (position.Position.EntryPrice - spreadAmount * BreakEvenTriggerSpreadMultiple))
                        {
                            l.LogTrace("Not moving Short SL until Ask is below " + (position.Position.EntryPrice - spreadAmount * BreakEvenTriggerSpreadMultiple) + " Spread amount: " + spreadAmount + " ask: " + position.Symbol.Ask);
                            break;
                        }
                    }

                    double x = Math.Min(Key.Number, (double)position.BarsSinceOpen(timeFrame)) / Key.Number;

                    double minTargetStopLoss = GetSL(x);

                    //l.LogTrace("[TSL] Key.Number: " + Key.Number + " barssinceopen: " + position.BarsSinceOpen(timeFrame) + "minTargetStopLoss: " + minTargetStopLoss + " x: " + x);

                    var oldStopLoss = position.MinStopLoss;

                    position.MinStopLoss = Cap(IncreaseOnly, position.MinStopLoss, minTargetStopLoss, position.Position.TradeType);
                    l.LogTrace("[TSL] NEW TSL based on " + DoubleFunctions.Lerp(Value.StartNumber, Value.Number, x) + "% profit: " + position.MinStopLoss);
                    break;
                default:
                    l.LogTrace("Unknown Key.Unit: " + Key.Unit);
                    break;
            }
        }
    }
}
