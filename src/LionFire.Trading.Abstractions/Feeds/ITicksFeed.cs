using System;

namespace LionFire.Trading.Feeds
{
    public interface ITicksFeed
    {
        event Action<ExchangeSymbolTick> Tick;
    }
}
