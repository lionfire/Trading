using System;
using System.Threading;
using System.Threading.Tasks;
using LionFire.Trading.Feeds.Models;
using LionFire.Trading.Feeds.Storage;
using LionFire.Trading.Feeds.Tracking;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Feeds.Collectors;

public abstract class FeedCollectorBase : IHostedService
{
    protected readonly ITimeSeriesStorage Storage;
    protected readonly ICvdTracker CvdTracker;
    protected readonly ILogger Logger;
    
    private CancellationTokenSource? _stoppingCts;

    protected FeedCollectorBase(
        ITimeSeriesStorage storage,
        ICvdTracker cvdTracker,
        ILogger logger)
    {
        Storage = storage;
        CvdTracker = cvdTracker;
        Logger = logger;
    }

    public abstract string ExchangeName { get; }
    
    protected abstract Task StartCollectingAsync(CancellationToken cancellationToken);
    protected abstract Task StopCollectingAsync();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Starting {Exchange} feed collector", ExchangeName);
        
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        try
        {
            await StartCollectingAsync(_stoppingCts.Token);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to start {Exchange} feed collector", ExchangeName);
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Stopping {Exchange} feed collector", ExchangeName);
        
        try
        {
            _stoppingCts?.Cancel();
            await StopCollectingAsync();
            await Storage.FlushAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error stopping {Exchange} feed collector", ExchangeName);
        }
        finally
        {
            _stoppingCts?.Dispose();
        }
    }

    protected async Task ProcessTradeAsync(
        string symbol,
        decimal price,
        decimal volume,
        bool isBuy,
        DateTime timestamp,
        decimal bidPrice,
        decimal askPrice,
        OrderBookDepth? orderBookDepth = null)
    {
        try
        {
            // Update CVD
            CvdTracker.UpdateCvd(symbol, volume, isBuy);
            var cvd = CvdTracker.GetCvd(symbol);
            
            // Create snapshot
            var snapshot = new MarketDataSnapshot
            {
                Exchange = ExchangeName,
                Symbol = symbol,
                Timestamp = timestamp,
                CumulativeVolumeDelta = cvd,
                LastTradePrice = price,
                LastTradeVolume = volume,
                LastTradeIsBuy = isBuy,
                BidPrice = bidPrice,
                AskPrice = askPrice,
                OrderBookDepth = orderBookDepth,
                Trigger = SnapshotTrigger.Trade
            };
            
            // Store snapshot
            await Storage.AppendAsync(snapshot);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, 
                "Failed to process trade for {Symbol} on {Exchange}", 
                symbol, ExchangeName);
        }
    }

    protected async Task ProcessOrderBookChangeAsync(
        string symbol,
        DateTime timestamp,
        decimal bidPrice,
        decimal askPrice,
        OrderBookDepth orderBookDepth)
    {
        try
        {
            var cvd = CvdTracker.GetCvd(symbol);
            
            var snapshot = new MarketDataSnapshot
            {
                Exchange = ExchangeName,
                Symbol = symbol,
                Timestamp = timestamp,
                CumulativeVolumeDelta = cvd,
                LastTradePrice = 0, // No trade occurred
                LastTradeVolume = 0,
                LastTradeIsBuy = false,
                BidPrice = bidPrice,
                AskPrice = askPrice,
                OrderBookDepth = orderBookDepth,
                Trigger = SnapshotTrigger.OrderBookChange
            };
            
            await Storage.AppendAsync(snapshot);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, 
                "Failed to process order book change for {Symbol} on {Exchange}", 
                symbol, ExchangeName);
        }
    }
}