#if cAlgo
using cAlgo.API;
using cAlgo.API.Internals;
using PositionDouble = cAlgo.API.Position;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading
{

    public static class TradeMath
    {

        public static bool AboutEquals(this double? value1, double? value2, double precalculatedContextualEpsilon = 0.0001)
        {
            if (!value1.HasValue && !value2.HasValue)
            {
                return true;
            }
            if (!value1.HasValue || !value2.HasValue)
            {
                return false;
            }
            return Math.Abs(value1.Value - value2.Value) <= precalculatedContextualEpsilon;
        }

        public static bool IsStopLossTriggered(double? stopLoss, PositionDouble position, Symbol symbol)
        {
            if (position.TradeType == TradeType.Buy)
            {
                // REVIEW
                if (stopLoss >= symbol.Bid)
                {
                    return true;
                }
            }
            else
            {
                if (stopLoss <= symbol.Ask)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsTakeProfitTriggered(double? takeProfit, PositionDouble position, Symbol symbol)
        {
            if (position.TradeType == TradeType.Buy)
            {
                // REVIEW
                if (takeProfit <= symbol.Bid)
                {
                    return true;
                }
            }
            else
            {
                if (takeProfit >= symbol.Ask)
                {
                    return true;
                }
            }
            return false;
        }


        public static double? Constrain(double? existingValue, double? minValue, double? maxValue, TradeType tradeType, bool preferMin = true)
        {
            /*if (minValue.HasValue && maxValue.HasValue)
            {
                if( minValue.Value > maxValue.Value)
                {
                    return double.NaN;
                }
            }*/

            if (minValue.HasValue && maxValue.HasValue && minValue.Value > maxValue.Value)
            {
                //l.LogTrace("minValue.Value "+minValue.Value +"> maxValue.Value "+maxValue.Value);
                return preferMin ? minValue : maxValue;
            }

            bool hasExisting = existingValue.HasValue && !double.IsNaN(existingValue.Value);

            if (!hasExisting)
            {
                if (minValue.HasValue && maxValue.HasValue)
                {
                    return preferMin ? minValue : maxValue;
                }
                else if (minValue.HasValue)
                {
                    return minValue;
                }
                else
                    return maxValue;
            }
            else
            {
                double newValue = existingValue.Value;
                if (minValue.HasValue)
                {
                    newValue = tradeType == TradeType.Buy ? Math.Max(newValue, minValue.Value) : Math.Min(newValue, minValue.Value);
                }
                if (minValue.HasValue && maxValue.HasValue)
                {
                    newValue = tradeType == TradeType.Buy ? Math.Min(newValue, maxValue.Value) : Math.Max(newValue, maxValue.Value);
                }
                return newValue;
            }
        }
    }
}
