using LionFire.Trading.Instruments;
using System;

namespace LionFire.Trading.Portfolios
{
    // TODO (STUB / HARDCODE)
    public static class InstrumentVolumeUtils // MOVE to Instruments / brokers area?
    {
        public static double GetMinTradeVolumeForSymbol(this SymbolHandle symbol)
        {
            if (Currencies.Symbols.Contains(symbol.LongAsset))
            {
                return 1000;
            }
            else if (Indices.Symbols.ContainsKey(symbol.LongAsset))
            {
                return 0.1; // Sometimes also 1
            }
            else if (symbol.LongAsset == "BTC")
            {
                return 0.01;
            }
            return double.NaN;
            // TODO ENH - allow configuration of this, to allow the user to choose the minimum for whatever broker they plan to trade with.
            //var broker = trade.Component.BacktestResult.Broker
        }
    }
}
