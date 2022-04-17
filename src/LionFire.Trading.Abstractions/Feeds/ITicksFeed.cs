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

        DateTimeOffset LastUpdate { get; }
    }

    

    public interface ISubExchangeId
    {
        /// <summary>
        /// Examples:
        ///  - Binance
        ///  - OANDA
        ///  - IC Markets
        /// </summary>
        string Exchange { get; }

        /// <summary>
        /// Examples:
        ///  - Futures
        ///  - Spot
        ///  - Margin
        ///  - Options
        /// </summary>
        string AccountTypeName { get; } // RENAME

        /// <summary>
        /// Examples:
        ///  - USD(S)-M
        ///  - COIN-M
        /// </summary>
        string AccountSubType { get; }

    }

    public interface IAccountId : ISubExchangeId
    {
        string AccountId { get; }
        string ApiKey { get; }

    }

    public interface IMarketId : ISubExchangeId
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

}
