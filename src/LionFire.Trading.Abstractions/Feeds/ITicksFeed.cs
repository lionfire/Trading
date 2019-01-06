using Microsoft.Extensions.Hosting;
using System;

namespace LionFire.Trading.Feeds
{
    public interface ITicksFeed : IHostedService
    {
        event Action<ExchangeSymbolTick> Tick;

        ExchangeSymbolTick LastTick(string symbol, string exchange = null);
    }
}
