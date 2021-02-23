using System;
using System.Collections.Generic;
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

    public interface ITicksFeed2
    {
        string Exchange { get; }

        /// <summary>
        /// May include: username, source (FIX vs websocket vs http).  To distinguish when different access points give different data contents, or timing of data.
        /// Format: comma separated.  If key/value is needed, in the form of "key: value"
        /// </summary>
        string AccessPoint { get; }

        event Action<IEnumerable<SymbolTick2>> MultiTick;

        DateTime LastUpdate { get; }
    }
}
