using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bybit.Net.Clients;
using Bybit.Net.Enums;
using Bybit.Net.Interfaces;
using Bybit.Net.Interfaces.Clients;
using Bybit.Net.Objects.Models.V5;
using CryptoExchange.Net.Objects.Sockets;
using LionFire.Trading.Feeds.Collectors;
using LionFire.Trading.Feeds.Configuration;
using LionFire.Trading.Feeds.Models;
using LionFire.Trading.Feeds.Storage;
using LionFire.Trading.Feeds.Tracking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Feeds.Bybit;

public class BybitFeedCollectorOptions : FeedCollectionOptions
{
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public bool UseTestnet { get; set; } = false;
}

public class BybitFeedCollector : FeedCollectorBase
{
    private readonly BybitFeedCollectorOptions _options;
    private readonly IBybitSocketClient _socketClient;
    private readonly IBybitRestClient _restClient;
    private readonly Dictionary<string, OrderBookDepth> _orderBooks = new();
    private readonly SemaphoreSlim _orderBookLock = new(1, 1);
    private readonly List<UpdateSubscription> _subscriptions = new();

    public override string ExchangeName => "Bybit";

    public BybitFeedCollector(
        IOptions<BybitFeedCollectorOptions> options,
        ITimeSeriesStorage storage,
        ICvdTracker cvdTracker,
        ILogger<BybitFeedCollector> logger)
        : base(storage, cvdTracker, logger)
    {
        _options = options.Value;
        
        // Create clients
        _socketClient = new BybitSocketClient();
        _restClient = new BybitRestClient();
        
        if (!string.IsNullOrEmpty(_options.ApiKey) && !string.IsNullOrEmpty(_options.ApiSecret))
        {
            _socketClient.V5Api.SetApiCredentials(_options.ApiKey, _options.ApiSecret);
            _restClient.V5Api.SetApiCredentials(_options.ApiKey, _options.ApiSecret);
        }
    }

    protected override async Task StartCollectingAsync(CancellationToken cancellationToken)
    {
        if (!_options.Symbols.Any())
        {
            Logger.LogWarning("No symbols configured for Bybit feed collector");
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
            
            // Bybit requires individual subscriptions per symbol
            foreach (var symbol in symbols)
            {
                var subscription = await _socketClient.V5LinearApi.SubscribeToTradeUpdatesAsync(
                    symbol,
                    async data =>
                    {
                        try
                        {
                            await ProcessTradeUpdatesAsync(symbol, data.Data);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "Error processing trade update for {Symbol}", symbol);
                        }
                    },
                    cancellationToken);

                if (subscription.Success)
                {
                    _subscriptions.Add(subscription.Data);
                    Logger.LogInformation("Subscribed to trades for {Symbol}", symbol);
                }
                else
                {
                    Logger.LogError("Failed to subscribe to trades for {Symbol}: {Error}", 
                        symbol, subscription.Error);
                }
            }
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
            
            foreach (var symbol in symbols)
            {
                var subscription = await _socketClient.V5LinearApi.SubscribeToOrderbookUpdatesAsync(
                    symbol,
                    25, // depth
                    async data =>
                    {
                        try
                        {
                            await ProcessOrderBookUpdateAsync(data.Data);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "Error processing order book update for {Symbol}", symbol);
                        }
                    },
                    cancellationToken);

                if (subscription.Success)
                {
                    _subscriptions.Add(subscription.Data);
                    Logger.LogInformation("Subscribed to order book for {Symbol}", symbol);
                }
                else
                {
                    Logger.LogError("Failed to subscribe to order book for {Symbol}: {Error}", 
                        symbol, subscription.Error);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error subscribing to order books");
            throw;
        }
    }

    private async Task ProcessTradeUpdatesAsync(string symbol, IEnumerable<BybitTrade> trades)
    {
        foreach (var trade in trades)
        {
            var isBuy = trade.Side == OrderSide.Buy;
            
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
                trade.Price,
                trade.Quantity,
                isBuy,
                trade.Timestamp,
                trade.Price, // TODO: Get actual bid
                trade.Price, // TODO: Get actual ask
                orderBookDepth);
        }
    }

    private async Task ProcessOrderBookUpdateAsync(BybitOrderbook orderBook)
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
                orderBook.Timestamp,
                bidPrice,
                askPrice,
                orderBookDepth);
        }
    }

    private OrderBookDepth CalculateOrderBookDepth(BybitOrderbook orderBook, decimal midPrice)
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

    private DepthLevel CalculateDepthAtLevel(BybitOrderbook orderBook, decimal midPrice, decimal percentage)
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