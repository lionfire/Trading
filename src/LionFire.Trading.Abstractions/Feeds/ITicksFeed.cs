using System;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading.Feeds
{

    public interface ITicksFeed
    {
        event Action<ExchangeSymbolTick> Tick;

        ExchangeSymbolTick LastTick(string symbol, string exchange = null);

        Task WaitForStarted(CancellationToken cancellationToken);
    }
}
