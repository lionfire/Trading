using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Objects.Models.Futures;
using Binance.Net.Objects.Models.Futures.Socket;
using CryptoExchange.Net.Objects.Sockets;
using LionFire.Trading.Feeds.Collectors;
using LionFire.Trading.Feeds.Configuration;
using LionFire.Trading.Feeds.Models;
using LionFire.Trading.Feeds.Storage;
using LionFire.Trading.Feeds.Tracking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Feeds.Binance;

public class BinanceFeedCollectorOptions : FeedCollectionOptions
{
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public bool UseTestnet { get; set; } = false;
}

public class BinanceFeedCollector : FeedCollectorBase
{
    private readonly BinanceFeedCollectorOptions _options;
    private readonly IBinanceSocketClient _socketClient;
    private readonly IBinanceRestClient _restClient;
    private readonly Dictionary<string, OrderBookDepth> _orderBooks = new();
    private readonly SemaphoreSlim _orderBookLock = new(1, 1);
    private readonly List<UpdateSubscription> _subscriptions = new();

    public override string ExchangeName => "Binance";

    public BinanceFeedCollector(
        IOptions<BinanceFeedCollectorOptions> options,
        ITimeSeriesStorage storage,
        ICvdTracker cvdTracker,
        ILogger<BinanceFeedCollector> logger)
        : base(storage, cvdTracker, logger)
    {
        _options = options.Value;
        
        // Create clients
        // TODO: Configure API credentials if needed
        _socketClient = new BinanceSocketClient();
        _restClient = new BinanceRestClient();
    }

    protected override async Task StartCollectingAsync(CancellationToken cancellationToken)
    {
        if (!_options.Symbols.Any())
        {
            Logger.LogWarning("No symbols configured for Binance feed collector");
            return;
        }

        // Subscribe to trades
        if (_options.CollectTrades)
        {
            await SubscribeToTradesAsync(cancellationToken);
        }

        // Subscribe to order book
        if (_options.CollectOrderBook)
        {
            await SubscribeToOrderBooksAsync(cancellationToken);
        }
    }

    private async Task SubscribeToTradesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var symbols = _options.Symbols.ToArray();
            // TODO: Update to match current Binance.Net API
            // var subscription = await _socketClient.UsdFuturesApi.SubscribeToAggregatedTradeUpdatesAsync(
            throw new NotImplementedException("Binance feed subscription needs to be updated for current Binance.Net version");
            /*var subscription = await _socketClient.UsdFuturesApi.SubscribeToAggregatedTradeUpdatesAsync(
                symbols,
                async data =>
                {
                    try
                    {
                        await ProcessTradeUpdateAsync(data.Data);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error processing trade update");
                    }
                },
                cancellationToken);

            if (subscription.Success)
            {
                _subscriptions.Add(subscription.Data);
                Logger.LogInformation("Subscribed to trades for {SymbolCount} symbols", symbols.Length);
            }
            else
            {
                Logger.LogError("Failed to subscribe to trades: {Error}", subscription.Error);
            }*/
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error subscribing to trades");
            throw;
        }
    }

    private async Task SubscribeToOrderBooksAsync(CancellationToken cancellationToken)
    {
        try
        {
            var symbols = _options.Symbols.ToArray();
            
            // Subscribe to order book updates
            var subscription = await _socketClient.UsdFuturesApi.SubscribeToPartialOrderBookUpdatesAsync(
                symbols,
                20, // depth levels
                100, // update interval ms
                async data =>
                {
                    try
                    {
                        await ProcessOrderBookUpdateAsync(data.Data);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error processing order book update");
                    }
                },
                cancellationToken);

            if (subscription.Success)
            {
                _subscriptions.Add(subscription.Data);
                Logger.LogInformation("Subscribed to order books for {SymbolCount} symbols", symbols.Length);
            }
            else
            {
                Logger.LogError("Failed to subscribe to order books: {Error}", subscription.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error subscribing to order books");
            throw;
        }
    }

    private async Task ProcessTradeUpdateAsync(dynamic trade)
    {
        var isBuy = !(bool)trade.BuyerIsMaker;
        var symbol = (string)trade.Symbol;
        var price = (decimal)trade.Price;
        var quantity = (decimal)trade.Quantity;
        var tradeTime = (DateTime)trade.TradeTime;
        
        // Get current order book data if available
        OrderBookDepth? orderBookDepth = null;
        await _orderBookLock.WaitAsync();
        try
        {
            _orderBooks.TryGetValue(symbol, out orderBookDepth);
        }
        finally
        {
            _orderBookLock.Release();
        }

        // For now, use trade price as bid/ask (will be updated with actual order book)
        await ProcessTradeAsync(
            symbol,
            price,
            quantity,
            isBuy,
            tradeTime,
            price, // TODO: Get actual bid
            price, // TODO: Get actual ask
            orderBookDepth);
    }

    private async Task ProcessOrderBookUpdateAsync(IBinanceEventOrderBook orderBook)
    {
        var bidPrice = orderBook.Bids.FirstOrDefault()?.Price ?? 0;
        var askPrice = orderBook.Asks.FirstOrDefault()?.Price ?? 0;
        
        if (bidPrice == 0 || askPrice == 0)
            return;

        var midPrice = (bidPrice + askPrice) / 2;
        var orderBookDepth = CalculateOrderBookDepth(orderBook, midPrice);

        // Update cached order book
        await _orderBookLock.WaitAsync();
        try
        {
            _orderBooks[orderBook.Symbol] = orderBookDepth;
        }
        finally
        {
            _orderBookLock.Release();
        }

        // Only create snapshot if not collecting on trade only
        if (!_options.CollectOnTradeOnly)
        {
            await ProcessOrderBookChangeAsync(
                orderBook.Symbol,
                DateTime.UtcNow,
                bidPrice,
                askPrice,
                orderBookDepth);
        }
    }

    private OrderBookDepth CalculateOrderBookDepth(IBinanceEventOrderBook orderBook, decimal midPrice)
    {
        var depth = new OrderBookDepth();
        
        // Calculate depth at each percentage level
        if (_options.OrderBookDepth.Collect01Percent)
            depth = depth with { Depth01Percent = CalculateDepthAtLevel(orderBook, midPrice, 0.001m) };
        
        if (_options.OrderBookDepth.Collect025Percent)
            depth = depth with { Depth025Percent = CalculateDepthAtLevel(orderBook, midPrice, 0.0025m) };
        
        if (_options.OrderBookDepth.Collect05Percent)
            depth = depth with { Depth05Percent = CalculateDepthAtLevel(orderBook, midPrice, 0.005m) };
        
        if (_options.OrderBookDepth.Collect075Percent)
            depth = depth with { Depth075Percent = CalculateDepthAtLevel(orderBook, midPrice, 0.0075m) };
        
        if (_options.OrderBookDepth.Collect1Percent)
            depth = depth with { Depth1Percent = CalculateDepthAtLevel(orderBook, midPrice, 0.01m) };
        
        if (_options.OrderBookDepth.Collect2Percent)
            depth = depth with { Depth2Percent = CalculateDepthAtLevel(orderBook, midPrice, 0.02m) };
        
        return depth;
    }

    private DepthLevel CalculateDepthAtLevel(IBinanceEventOrderBook orderBook, decimal midPrice, decimal percentage)
    {
        var bidThreshold = midPrice * (1 - percentage);
        var askThreshold = midPrice * (1 + percentage);
        
        var bidVolume = orderBook.Bids
            .Where(b => b.Price >= bidThreshold)
            .Sum(b => b.Quantity);
        
        var askVolume = orderBook.Asks
            .Where(a => a.Price <= askThreshold)
            .Sum(a => a.Quantity);
        
        var bestBidAtLevel = orderBook.Bids
            .Where(b => b.Price >= bidThreshold)
            .MinBy(b => Math.Abs(b.Price - bidThreshold))?.Price ?? 0;
        
        var bestAskAtLevel = orderBook.Asks
            .Where(a => a.Price <= askThreshold)
            .MinBy(a => Math.Abs(a.Price - askThreshold))?.Price ?? 0;
        
        return new DepthLevel
        {
            BidVolume = bidVolume,
            AskVolume = askVolume,
            BidPrice = bestBidAtLevel,
            AskPrice = bestAskAtLevel
        };
    }

    protected override async Task StopCollectingAsync()
    {
        foreach (var subscription in _subscriptions)
        {
            await subscription.CloseAsync();
        }
        _subscriptions.Clear();
        
        _socketClient?.Dispose();
        _restClient?.Dispose();
    }
}