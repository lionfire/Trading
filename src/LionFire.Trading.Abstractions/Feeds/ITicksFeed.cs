using LionFire.Trading.Exchanges;
using LionFire.Trading.Markets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading.Feeds;

public interface ITicksFeed
{
    event Action<ExchangeSymbolTick> Tick;

    ExchangeSymbolTick LastTick(string symbol, string exchange = null);

    Task WaitForStarted(CancellationToken cancellationToken);
}

public interface ITicksFeed2
{
    ISymbolIdTranslator SymbolIdTranslator { get; }
    string Exchange { get; }

    /// <summary>
    /// May include: username, source (FIX vs websocket vs http).  To distinguish when different access points give different data contents, or timing of data.
    /// Format: comma separated.  If key/value is needed, in the form of "key: value"
    /// </summary>
    string AccessPoint { get; }

    event Action<IEnumerable<NativeSymbolTick>> NativeMultiTick;
    event Action<IEnumerable<SymbolTick3>> MultiTick;

    DateTimeOffset LastUpdate { get; }
}

public interface IAccountId : ISubExchangeId // REVIEW
{
    string AccountId { get; }
    string ApiKey { get; }

}

public interface IMarketId : ISubExchangeId // REVIEW
{
    /// <summary>
    /// Examples:
    ///  - BTCUSDT
    ///  - XAUUSD
    /// </summary>
    string Symbol { get; }
}

public interface IAccountMarketId : IMarketId, IAccountId { }


public interface ISubExchangeAccount
{

}

public interface IAccountMarket
{
    // Reference to  ISubExchangeAccount

}


public interface IMarketFeed
{

}

public interface IBidAskFeed
{

}

public interface IMarkPriceFeed
{

}

public class FeedProvider
{

}

