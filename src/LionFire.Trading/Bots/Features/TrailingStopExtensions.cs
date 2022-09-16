#if cAlgo
using cAlgo.API;
using cAlgo.API.Internals;
#endif
using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Trading.Bots.Features
{
    public static class TrailingStopExtensions
    {
        public static void TrailLongStopsFromLastBar(this BotBase bot, double trailAmount)
        {
            if (trailAmount <= 0) return;
            MarketSeries series = bot.MarketSeries;
            double high = series.High.Last(1);
            foreach (var position in bot.BotLongPositions)
            {
                TrailStops(bot, position, high, trailAmount);
            }
        }
        public static void TrailShortStopsFromLastBar(this BotBase bot, double trailAmount)
        {
            if (trailAmount <= 0) return;
            MarketSeries series = bot.MarketSeries;
            double low = series.Low.Last(1);
            foreach (var position in bot.BotShortPositions)
            {
                TrailStops(bot, position, low, trailAmount);
            }
        }
        public static void TrailStops(this BotBase bot, PositionDouble position, double newExtreme, double trailAmount)
        {
            if (trailAmount <= 0) return;
            if (position.TradeType == TradeType.Buy)
            {
                var trailPrice = newExtreme - trailAmount;
                if (!position.StopLoss.HasValue || trailPrice > position.StopLoss.Value)
                {
                    bot.ModifyPosition(position, trailPrice, position.TakeProfit);
                }
            }
            else
            {
                var trailPrice = newExtreme + trailAmount;
                if (!position.StopLoss.HasValue || trailPrice < position.StopLoss.Value)
                {
                    bot.ModifyPosition(position, trailPrice, position.TakeProfit);
                }
            }
        }
    }
}
