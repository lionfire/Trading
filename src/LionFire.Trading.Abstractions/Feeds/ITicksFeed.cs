using System;

namespace LionFire.Trading.Feeds
{
    public interface ITicksFeed
    {
        event Action<ExchangeSymbolTick> Tick;

        ExchangeSymbolTick LastTick(string symbol, string exchange = null);
    }
}
